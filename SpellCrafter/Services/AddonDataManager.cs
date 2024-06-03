using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SpellCrafter.Data;
using SpellCrafter.Enums;
using SpellCrafter.Models;
using SQLiteNetExtensions.Extensions;

namespace SpellCrafter.Services
{
    public static class AddonDataManager
    {
        public static List<Addon> InstalledAddons { get; set; }
        public static List<Addon> OnlineAddons { get; set; }

        static AddonDataManager() // TODO move to startup waiting window
        {
            EsoDataConnection.CreateTablesIfNotExists();

            using var db = new EsoDataConnection();
            InstalledAddons = GetAllLocalAddonsWithDetails(db);
            OnlineAddons = GetAllOnlineAddonsWithDetails(db);
            SyncAddonLists();
        }

        private static void UpdateAddonsState() // TODO move to startup waiting window
        {
            foreach (var installedAddon in InstalledAddons)
            {
                if (installedAddon.AddonState is AddonState.LatestVersion or AddonState.Outdated &&
                    Addon.CompareVersions(installedAddon.Version, installedAddon.LatestVersion) < 0)
                {
                    installedAddon.AddonState = AddonState.Outdated;
                }
            }
        }

        public static void UpdateLocalAddonList(EsoDataConnection db, List<Addon> addons)
        {
            var filteredAddons = FilterAddonsWithLatestVersion(addons);
            UpdateCommonAddons(db, filteredAddons);
            UpdateLocalAddons(db, filteredAddons);
            RemoveUnusedCommonAddons(db);

            UpdateAuthors(db, filteredAddons);
            RemoveUnusedAuthors(db);
            UpdateCategories(db, filteredAddons);
            RemoveUnusedCategories(db);

            InstalledAddons = GetAllLocalAddonsWithDetails(db);
            SyncAddonLists();
        }

        public static void UpdateOnlineAddonList(EsoDataConnection db, List<Addon> addons)
        {
            var filteredAddons = FilterAddonsWithLatestVersion(addons);
            UpdateCommonAddons(db, filteredAddons);
            UpdateOnlineAddons(db, filteredAddons);
            RemoveUnusedCommonAddons(db);

            UpdateAuthors(db, filteredAddons);
            RemoveUnusedAuthors(db);
            UpdateCategories(db, filteredAddons);
            RemoveUnusedCategories(db);

            OnlineAddons = GetAllOnlineAddonsWithDetails(db);
            SyncAddonLists();
        }

        private static List<Addon> FilterAddonsWithLatestVersion(List<Addon> addons)
        {
            return addons.GroupBy(addon => (addon.Name, GetAuthorsKey(addon.Authors)))
                .Select(g =>
                {
                    return g.Aggregate((currentMax, x) =>
                        Addon.CompareVersions(currentMax, x) > 0 ? currentMax : x);
                }).ToList();
        }

        public static void SyncAddonLists()
        {
            var addonMap = new Dictionary<string, Addon>();

            foreach (var addon in InstalledAddons.Where(addon => !addonMap.ContainsKey(addon.Name)))
                addonMap.Add(addon.Name, addon);

            for (var i = 0; i < OnlineAddons.Count; ++i)
            {
                if (addonMap.TryGetValue(OnlineAddons[i].Name, out var matchedAddon))
                {
                    matchedAddon.UniqueId = OnlineAddons[i].UniqueId;
                    matchedAddon.LatestVersion = OnlineAddons[i].LatestVersion;
                    matchedAddon.DisplayedLatestVersion = OnlineAddons[i].DisplayedLatestVersion;
                    matchedAddon.Categories = OnlineAddons[i].Categories;
                    OnlineAddons[i] = matchedAddon;
                }
                else
                    addonMap.Add(OnlineAddons[i].Name, OnlineAddons[i]);
            }
            
            UpdateAddonsState();
        }

        public static List<Addon> GetAllLocalAddonsWithDetails(EsoDataConnection db)
        {
            var localAddons = db.GetAllWithChildren<LocalAddon>(recursive: true);

            var addonAuthors = db.Table<AddonAuthor>().ToList();
            var authors = db.Table<Author>().ToList().ToDictionary(a => a.Id);
            var addonCategories = db.Table<AddonCategory>().ToList();
            var categories = db.Table<Category>().ToList().ToDictionary(c => c.Id);

            return localAddons.Select(localAddon =>
            {
                var commonAddon = localAddon.CommonAddon;

                var addonAuthorsList = addonAuthors.Where(aa => aa.CommonAddonId == commonAddon.Id)
                    .Select(aa => authors[aa.AuthorId])
                    .ToList();
                var addonCategoriesList = addonCategories.Where(ac => ac.CommonAddonId == commonAddon.Id)
                    .Select(ac => categories[ac.CategoryId])
                    .ToList();

                return new Addon
                {
                    CommonAddonId = localAddon.CommonAddonId,
                    Name = commonAddon.Name,
                    Title = commonAddon.Title,
                    Description = commonAddon.Description,
                    AddonState = localAddon.State,
                    InstallationMethod = localAddon.InstallationMethod,
                    Authors = new ObservableCollection<Author>(addonAuthorsList),
                    Version = localAddon.Version,
                    DisplayedVersion = localAddon.DisplayedVersion,
                    Dependencies = commonAddon.Dependencies
                };
            }).ToList();
        }

        public static List<Addon> GetAllOnlineAddonsWithDetails(EsoDataConnection db) // TODO change to pagination
        {
            var onlineAddons = db.GetAllWithChildren<OnlineAddon>(recursive: true);

            var addonAuthors = db.Table<AddonAuthor>().ToList();
            var authors = db.Table<Author>().ToList().ToDictionary(a => a.Id);
            var addonCategories = db.Table<AddonCategory>().ToList();
            var categories = db.Table<Category>().ToList().ToDictionary(c => c.Id);

            return onlineAddons.Select(onlineAddon =>
            {
                var commonAddon = onlineAddon.CommonAddon;

                var addonAuthorsList = addonAuthors.Where(aa => aa.CommonAddonId == commonAddon.Id)
                    .Select(aa => authors[aa.AuthorId])
                    .ToList();
                var addonCategoriesList = addonCategories.Where(ac => ac.CommonAddonId == commonAddon.Id)
                    .Select(ac => categories[ac.CategoryId])
                    .ToList();

                return new Addon
                {
                    CommonAddonId = onlineAddon.CommonAddonId,
                    Name = commonAddon.Name,
                    Title = commonAddon.Title,
                    Description = commonAddon.Description,
                    AddonState = AddonState.NotInstalled,
                    Categories = new ObservableCollection<Category>(addonCategoriesList),
                    Authors = new ObservableCollection<Author>(addonAuthorsList),
                    UniqueId = onlineAddon.UniqueId,
                    LatestVersion = onlineAddon.LatestVersion,
                    DisplayedLatestVersion = onlineAddon.DisplayedLatestVersion,
                    Dependencies = commonAddon.Dependencies
                };
            }).ToList();
        }

        public static string GetAuthorsKey(IEnumerable<Author> authors) =>
            string.Join(',', authors.Select(author => author.Name).OrderBy(name => name));

        private static bool UpdateCommonAddonIfChanged(CommonAddon commonAddon, Addon updatedAddon)
        {
            var isChanged = false;

            if (commonAddon.Description != updatedAddon.Description)
            {
                commonAddon.Description = updatedAddon.Description;
                isChanged = true;
            }

            if (commonAddon.Title != updatedAddon.Title)
            {
                commonAddon.Title = updatedAddon.Title;
                isChanged = true;
            }
            // TODO update dependencies

            return isChanged;
        }

        private static void UpdateCommonAddons(EsoDataConnection db, List<Addon> updatedAddons)
        {
            if (updatedAddons.Count == 0)
                return;

            var commonAddons = db.Table<CommonAddon>().ToList().ToDictionary(
                addon => (addon.Name, GetAuthorsKey(addon.Authors)), author => author);

            var toInsert = new List<CommonAddon>();
            var toUpdate = new List<CommonAddon>();

            foreach (var addon in updatedAddons)
            {
                var authorsKey = GetAuthorsKey(addon.Authors);
                var addonKey = (addon.Name, authorsKey);

                if (!commonAddons.TryGetValue(addonKey, out var dbAddon))
                {
                    dbAddon = new CommonAddon
                    {
                        Name = addon.Name,
                        Title = addon.Title,
                        Description = addon.Description,
                        Authors = [.. addon.Authors]
                        // TODO Add dependencies
                    };
                    toInsert.Add(dbAddon);
                }
                else
                {
                    addon.CommonAddonId = dbAddon.Id;
                    if (UpdateCommonAddonIfChanged(dbAddon, addon))
                        toUpdate.Add(dbAddon);
                }
            }

            if (toInsert.Count != 0)
            {
                db.InsertAll(toInsert);

                foreach (var addon in toInsert)
                {
                    var authorsKey = (addon.Name, GetAuthorsKey(addon.Authors));
                    var updateAddon = updatedAddons.First(updatedAddon =>
                        (updatedAddon.Name, GetAuthorsKey(updatedAddon.Authors)) == authorsKey);
                    updateAddon.CommonAddonId = addon.Id;
                }
            }

            if (toUpdate.Count != 0)
                db.UpdateAll(toUpdate);
        }

        private static bool UpdateLocalAddonIfChanged(LocalAddon localAddon, Addon updatedAddon)
        {
            var isChanged = false;

            if (localAddon.Version != updatedAddon.Version)
            {
                localAddon.Version = updatedAddon.Version;
                isChanged = true;
            }

            if (localAddon.DisplayedVersion != updatedAddon.DisplayedVersion)
            {
                localAddon.DisplayedVersion = updatedAddon.DisplayedVersion;
                isChanged = true;
            }

            if (localAddon.State != updatedAddon.AddonState)
            {
                localAddon.State = updatedAddon.AddonState;
                isChanged = true;
            }

            if (localAddon.InstallationMethod != updatedAddon.InstallationMethod)
            {
                localAddon.InstallationMethod = updatedAddon.InstallationMethod;
                isChanged = true;
            }

            return isChanged;
        }

        private static void UpdateLocalAddons(EsoDataConnection db, List<Addon> updatedAddons)
        {
            var localAddons = db.Table<LocalAddon>().ToList();
            var toUpdate = new List<LocalAddon>();

            foreach (var addon in localAddons)
            {
                var updatedAddon = updatedAddons.FirstOrDefault(a => a.CommonAddonId == addon.CommonAddonId);
                if (updatedAddon != null)
                {
                    if (UpdateLocalAddonIfChanged(addon, updatedAddon))
                        toUpdate.Add(addon);
                }
                else
                    db.Delete(addon);
            }

            var localCommonAddonIds = new HashSet<int>(localAddons.Select(a => a.CommonAddonId));
            var toInsert = (from updatedAddon in updatedAddons
                where updatedAddon.CommonAddonId != null && !localCommonAddonIds.Contains(updatedAddon.CommonAddonId.Value)
                select new LocalAddon
                {
                    CommonAddonId = updatedAddon.CommonAddonId!.Value,
                    Version = updatedAddon.Version,
                    DisplayedVersion = updatedAddon.DisplayedVersion,
                    State = updatedAddon.AddonState,
                    InstallationMethod = updatedAddon.InstallationMethod
                }).ToList();

            if (toUpdate.Count != 0)
                db.UpdateAll(toUpdate);

            if (toInsert.Count != 0)
                db.InsertAll(toInsert);
        }

        private static bool UpdateOnlineAddonIfChanged(OnlineAddon onlineAddon, Addon updatedAddon)
        {
            var isChanged = false;

            if (onlineAddon.LatestVersion != updatedAddon.LatestVersion)
            {
                onlineAddon.LatestVersion = updatedAddon.LatestVersion;
                isChanged = true;
            }

            if (onlineAddon.DisplayedLatestVersion != updatedAddon.DisplayedLatestVersion)
            {
                onlineAddon.DisplayedLatestVersion = updatedAddon.DisplayedLatestVersion;
                isChanged = true;
            }

            return isChanged;
        }

        private static void UpdateOnlineAddons(EsoDataConnection db, List<Addon> updatedAddons)
        {
            var onlineAddons = db.Table<OnlineAddon>().ToList();
            var toUpdate = new List<OnlineAddon>();

            foreach (var addon in onlineAddons)
            {
                var updatedAddon = updatedAddons.FirstOrDefault(a => a.CommonAddonId == addon.CommonAddonId);
                if (updatedAddon != null)
                {
                    if (UpdateOnlineAddonIfChanged(addon, updatedAddon))
                        toUpdate.Add(addon);
                }
                else
                    db.Delete(addon);
            }

            var onlineCommonAddonIds = new HashSet<int>(onlineAddons.Select(a => a.CommonAddonId));
            var toInsert = (from updatedAddon in updatedAddons
                where updatedAddon.CommonAddonId != null && !onlineCommonAddonIds.Contains(updatedAddon.CommonAddonId.Value)
                select new OnlineAddon
                {
                    CommonAddonId = updatedAddon.CommonAddonId!.Value,
                    LatestVersion = updatedAddon.LatestVersion,
                    DisplayedLatestVersion = updatedAddon.DisplayedLatestVersion
                }).ToList();

            if (toUpdate.Count != 0)
                db.UpdateAll(toUpdate);

            if (toInsert.Count != 0)
                db.InsertAll(toInsert);
        }

        private static void RemoveUnusedCommonAddons(EsoDataConnection db)
        {
            var usedCommonAddonIds = new HashSet<int>(
                db.Table<LocalAddon>().Select(localAddon => localAddon.CommonAddonId)
                    .Union(db.Table<OnlineAddon>().Select(onlineAddon => onlineAddon.CommonAddonId))
            );

            var unusedCommonAddons = db.Table<CommonAddon>().Where(ca => !usedCommonAddonIds.Contains(ca.Id)).ToList();

            foreach (var commonAddon in unusedCommonAddons)
            {
                var addonAuthors = db.Table<AddonAuthor>().Where(addonAuthor => addonAuthor.CommonAddonId == commonAddon.Id).ToList();
                foreach (var addonAuthor in addonAuthors)
                {
                    db.Delete(addonAuthor);
                }

                var addonCategories = db.Table<AddonCategory>().Where(addonCategory => addonCategory.CommonAddonId == commonAddon.Id).ToList();
                foreach (var addonCategory in addonCategories)
                {
                    db.Delete(addonCategory);
                }

                db.Delete(commonAddon);
            }
        }

        private static void UpdateAuthors(EsoDataConnection db, List<Addon> updatedAddons)
        {
            var authors = db.GetAllWithChildren<Author>().ToList().ToDictionary(author => author.Name, author => author);

            foreach (var addon in updatedAddons)
            {
                var commonAddon = db.GetWithChildren<CommonAddon>(addon.CommonAddonId);
                commonAddon.Authors = [];

                var toInsert = new List<Author>();

                foreach (var addonAuthor in addon.Authors)
                {
                    var lowerAuthorName = addonAuthor.Name.ToLower();
                    if (!authors.TryGetValue(lowerAuthorName, out var author))
                    {
                        author = new Author { Name = lowerAuthorName };
                        toInsert.Add(author);
                        authors[author.Name] = author;
                    }

                    commonAddon.Authors.Add(author);
                }

                if (toInsert.Count != 0)
                    db.InsertAll(toInsert);

                db.UpdateWithChildren(commonAddon);
            }
        }

        private static void UpdateCategories(EsoDataConnection db, List<Addon> updatedAddons)
        {
            var categories = db.GetAllWithChildren<Category>().ToList().ToDictionary(category => category.Name, category => category);

            foreach (var addon in updatedAddons)
            {
                var commonAddon = db.GetWithChildren<CommonAddon>(addon.CommonAddonId);
                commonAddon.Categories = [];

                var toInsert = new List<Category>();

                foreach (var addonCategory in addon.Categories)
                {
                    if (!categories.TryGetValue(addonCategory.Name, out var category))
                    {
                        category = new Category { Name = addonCategory.Name };
                        toInsert.Add(category);
                        categories[category.Name] = category;
                    }

                    commonAddon.Categories.Add(category);
                }

                if (toInsert.Count != 0)
                    db.InsertAll(toInsert);

                db.UpdateWithChildren(commonAddon);
            }
        }

        private static void RemoveUnusedAuthors(EsoDataConnection db)
        {
            var unusedAuthors = db.GetAllWithChildren<Author>().Where(a => a.CommonAddons.Count == 0).ToList();
            
            foreach (var author in unusedAuthors)
                db.Delete(author);
        }

        private static void RemoveUnusedCategories(EsoDataConnection db)
        {
            var unusedCategories = db.GetAllWithChildren<Category>().Where(a => a.CommonAddons.Count == 0).ToList();

            foreach (var category in unusedCategories)
                db.Delete(category);
        }
    }
}
