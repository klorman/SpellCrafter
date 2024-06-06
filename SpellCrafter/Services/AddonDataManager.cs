using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Avalonia.Controls;
using DynamicData;
using SpellCrafter.Data;
using SpellCrafter.Enums;
using SpellCrafter.Models;
using SQLiteNetExtensions.Extensions;

namespace SpellCrafter.Services
{
    public static class AddonDataManager
    {
        public static RangedObservableCollection<Addon> InstalledAddons { get; set; } = [];
        public static RangedObservableCollection<Addon> OnlineAddons { get; set; } = [];

        static AddonDataManager() // TODO move to startup waiting window
        {
            EsoDataConnection.CreateTablesIfNotExists();

            using var db = new EsoDataConnection();
            InstalledAddons.Refresh(GetAllLocalAddonsWithDetails(db));
            OnlineAddons.Refresh(GetAllOnlineAddonsWithDetails(db));
            SyncAddonLists(db);

            CheckAddonInstallationErrors(db);
        }

        private static void CheckAddonVersions(EsoDataConnection db) // TODO move to startup waiting window
        {
            var toUpdate = new List<Addon>();
            foreach (var installedAddon in InstalledAddons)
            {
                if (installedAddon.State is AddonState.LatestVersion &&
                    AddonVersionComparer.CompareVersions(installedAddon.Version, installedAddon.LatestVersion) < 0)
                {
                    installedAddon.State = AddonState.Outdated;
                    toUpdate.Add(installedAddon);
                }
            }

            InsertOrUpdateLocalAddons(db, toUpdate);
        }

        private static void CheckAddonInstallationErrors(EsoDataConnection db)
        {
            var incorrectAddons =
                InstalledAddons.Where(installedAddon => installedAddon.State == AddonState.Installing).ToList();
            foreach (var addon in incorrectAddons)
                addon.State = AddonState.InstallationError;

            InsertOrUpdateLocalAddons(db, incorrectAddons);
        }

        public static void UpdateInstalledAddonsInfo(EsoDataConnection db, List<Addon> addons)
        {
            var filteredAddons = FilterAddonsWithLatestVersion(addons);
            InsertOrUpdateCommonAddons(db, filteredAddons);
            UpdateAddonDependencies(db, filteredAddons);
            RefreshLocalAddons(db, filteredAddons);
            RemoveUnusedCommonAddons(db);

            InsertOrUpdateAuthors(db, filteredAddons);
            RemoveUnusedAuthors(db);
            InsertOrUpdateCategories(db, filteredAddons);
            RemoveUnusedCategories(db);

            InstalledAddons.Refresh(GetAllLocalAddonsWithDetails(db));
            SyncAddonLists(db);
        }

        public static void UpdateOnlineAddonsInfo(EsoDataConnection db, List<Addon> addons)
        {
            var filteredAddons = FilterAddonsWithLatestVersion(addons);
            InsertOrUpdateCommonAddons(db, filteredAddons);
            UpdateAddonDependencies(db, filteredAddons);
            RefreshOnlineAddons(db, filteredAddons);
            RemoveUnusedCommonAddons(db);

            InsertOrUpdateAuthors(db, filteredAddons);
            RemoveUnusedAuthors(db);
            InsertOrUpdateCategories(db, filteredAddons);
            RemoveUnusedCategories(db);

            OnlineAddons.Refresh(GetAllOnlineAddonsWithDetails(db));
            SyncAddonLists(db);
        }

        private static List<Addon> FilterAddonsWithLatestVersion(List<Addon> addons)
        {
            return addons.GroupBy(addon => (addon.Name, GetAuthorsKey(addon.Authors)))
                .Select(g =>
                {
                    return g.Aggregate((currentMax, x) =>
                        AddonVersionComparer.CompareVersions(currentMax, x) > 0 ? currentMax : x);
                }).ToList();
        }

        public static void SyncAddonLists(EsoDataConnection db)
        {
            var addonMap = InstalledAddons.ToDictionary(addon => (addon.Name, GetAuthorsKey(addon.Authors)), addon => addon);
            
            for (var i = 0; i < OnlineAddons.Count; ++i)
            {
                var authorsKey = GetAuthorsKey(OnlineAddons[i].Authors);
                var addonKey = (OnlineAddons[i].Name, authorsKey);

                if (!addonMap.TryGetValue(addonKey, out var matchedAddon)) continue;

                matchedAddon.UniqueId = OnlineAddons[i].UniqueId;
                matchedAddon.LatestVersion = OnlineAddons[i].LatestVersion;
                matchedAddon.DisplayedLatestVersion = OnlineAddons[i].DisplayedLatestVersion;
                matchedAddon.Categories = OnlineAddons[i].Categories;
                OnlineAddons[i] = matchedAddon;
            }
            
            CheckAddonVersions(db);
        }

        public static RangedObservableCollection<Addon> GetAllLocalAddonsWithDetails(EsoDataConnection db)
        {
            var localAddons = db.GetAllWithChildren<LocalAddon>(recursive: true);

            var addonAuthors = db.Table<AddonAuthor>().ToList();
            var authors = db.Table<Author>().ToList().ToDictionary(a => a.Id);

            var localAddonIds = new HashSet<int>(localAddons.Select(la => la.CommonAddonId));
            var addonDependencies = db.Table<AddonDependency>().Where(ad => localAddonIds.Contains(ad.CommonAddonId)).ToList();

            var dependentAddonIds = addonDependencies.Select(ad => ad.DependentCommonAddonId).Distinct().ToList();
            var commonAddons = db.Table<CommonAddon>().Where(ca => dependentAddonIds.Contains(ca.Id)).ToList().ToDictionary(ca => ca.Id);

            return new RangedObservableCollection<Addon>(localAddons.Select(localAddon =>
            {
                var commonAddon = localAddon.CommonAddon;

                var addonAuthorsList = addonAuthors.Where(aa => aa.CommonAddonId == commonAddon.Id)
                    .Select(aa => authors[aa.AuthorId])
                    .ToList();

                var dependencies = addonDependencies.Where(ad => ad.CommonAddonId == commonAddon.Id)
                    .Select(ad => commonAddons.GetValueOrDefault(ad.DependentCommonAddonId))
                    .Where(ad => ad != null)
                    .Select(ad => ad!)
                    .ToList();

                return new Addon
                {
                    CommonAddonId = localAddon.CommonAddonId,
                    Name = commonAddon.Name,
                    Title = commonAddon.Title,
                    Description = commonAddon.Description,
                    State = localAddon.State,
                    InstallationMethod = localAddon.InstallationMethod,
                    Authors = new ObservableCollection<Author>(addonAuthorsList),
                    Version = localAddon.Version,
                    DisplayedVersion = localAddon.DisplayedVersion,
                    Dependencies = dependencies
                };
            }).ToList());
        }

        public static RangedObservableCollection<Addon> GetAllOnlineAddonsWithDetails(EsoDataConnection db) // TODO change to pagination
        {
            var onlineAddons = db.GetAllWithChildren<OnlineAddon>(recursive: true);

            var addonAuthors = db.Table<AddonAuthor>().ToList();
            var authors = db.Table<Author>().ToList().ToDictionary(a => a.Id);
            var addonCategories = db.Table<AddonCategory>().ToList();
            var categories = db.Table<Category>().ToList().ToDictionary(c => c.Id);

            var onlineAddonIds = new HashSet<int>(onlineAddons.Select(la => la.CommonAddonId));
            var addonDependencies = db.Table<AddonDependency>().Where(ad => onlineAddonIds.Contains(ad.CommonAddonId)).ToList();
            var dependentAddonIds = addonDependencies.Select(ad => ad.DependentCommonAddonId).Distinct().ToList();
            var commonAddons = db.Table<CommonAddon>().Where(ca => dependentAddonIds.Contains(ca.Id)).ToList().ToDictionary(ca => ca.Id);

            return new RangedObservableCollection<Addon>(onlineAddons.Select(onlineAddon =>
            {
                var commonAddon = onlineAddon.CommonAddon;

                var addonAuthorsList = addonAuthors.Where(aa => aa.CommonAddonId == commonAddon.Id)
                    .Select(aa => authors[aa.AuthorId])
                    .ToList();
                var addonCategoriesList = addonCategories.Where(ac => ac.CommonAddonId == commonAddon.Id)
                    .Select(ac => categories[ac.CategoryId])
                    .ToList();

                var dependencies = addonDependencies.Where(ad => ad.CommonAddonId == commonAddon.Id)
                    .Select(ad => commonAddons.GetValueOrDefault(ad.DependentCommonAddonId))
                    .Where(ad => ad != null)
                    .Select(ad => ad!)
                    .ToList();

                return new Addon
                {
                    CommonAddonId = onlineAddon.CommonAddonId,
                    Name = commonAddon.Name,
                    Title = commonAddon.Title,
                    Description = commonAddon.Description,
                    State = AddonState.NotInstalled,
                    Categories = new ObservableCollection<Category>(addonCategoriesList),
                    Authors = new ObservableCollection<Author>(addonAuthorsList),
                    UniqueId = onlineAddon.UniqueId,
                    LatestVersion = onlineAddon.LatestVersion,
                    DisplayedLatestVersion = onlineAddon.DisplayedLatestVersion,
                    Dependencies = dependencies
                };
            }).ToList());
        }

        public static string GetAuthorsKey(IEnumerable<Author> authors) => // TODO replace with ids
            string.Join(',', authors.Select(author => author.Name).OrderBy(name => name));

        private static bool UpdateCommonAddonFieldsIfChanged(CommonAddon commonAddon, ICommonAddon updatedAddon)
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

        private static void InsertOrUpdateCommonAddons(EsoDataConnection db, List<Addon> updatedAddons)
        {
            if (updatedAddons.Count == 0)
                return;

            var commonAddons = db.GetAllWithChildren<CommonAddon>(recursive: true);
            var commonAddonsDictionary = commonAddons.ToDictionary(
                addon => (addon.Name, GetAuthorsKey(addon.Authors)), addon => addon);

            var toInsert = new List<CommonAddon>();
            var toUpdate = new List<CommonAddon>();

            foreach (var addon in updatedAddons)
            {
                var authorsKey = GetAuthorsKey(addon.Authors);
                var addonKey = (addon.Name, authorsKey);

                if (!commonAddonsDictionary.TryGetValue(addonKey, out var dbAddon))
                {
                    dbAddon = new CommonAddon
                    {
                        Name = addon.Name,
                        Title = addon.Title,
                        Description = addon.Description,
                        Authors = [.. addon.Authors]
                    };
                    toInsert.Add(dbAddon);
                }
                else
                {
                    addon.CommonAddonId = dbAddon.Id;
                    if (UpdateCommonAddonFieldsIfChanged(dbAddon, addon))
                        toUpdate.Add(dbAddon);
                }
            }

            foreach (var addon in updatedAddons)
            {
                foreach (var addonDependency in addon.Dependencies)
                {
                    var commonAddon = commonAddons.Where(a => a.Name == addonDependency.Name)
                        .Select(a => new{ Addon = a, Version = a.OnlineAddon?.LatestVersion })
                        .OrderByDescending(a => a.Version, new AddonVersionComparer())
                        .Select(a => a.Addon)
                        .FirstOrDefault(); // TODO ask user which addon to choose
                    if (commonAddon == null)
                    {
                        var index = toInsert
                            .Select((a, idx) => new { Addon = a, Index = idx })
                            .OrderByDescending(a => a.Addon.OnlineAddon?.LatestVersion, new AddonVersionComparer())
                            .FirstOrDefault(a => a.Addon.Name == addonDependency.Name)?.Index ?? -1;
                        if (index != -1)
                            toInsert[index] = addonDependency;
                            
                        toInsert.Add(addonDependency);
                    }
                    else
                        addonDependency.Id = commonAddon.Id;
                }
            }

            if (toInsert.Count != 0)
            {
                db.InsertAll(toInsert);

                foreach (var addon in toInsert)
                {
                    var authorsKey = (addon.Name, GetAuthorsKey(addon.Authors));
                    var updateAddon = updatedAddons.FirstOrDefault(updatedAddon =>
                        (updatedAddon.Name, GetAuthorsKey(updatedAddon.Authors)) == authorsKey);
                    if (updateAddon != null)
                        updateAddon.CommonAddonId = addon.Id;
                }
            }

            if (toUpdate.Count != 0)
                db.UpdateAll(toUpdate);
        }

        private static void UpdateAddonDependencies(EsoDataConnection db, List<Addon> updatedAddons)
        {
            foreach (var updatedAddon in updatedAddons)
            {
                var existingDependencies = db.Table<AddonDependency>()
                    .Where(dependency => dependency.CommonAddonId == updatedAddon.CommonAddonId)
                    .ToList();
                var currentDependencyIds = updatedAddon.Dependencies.Select(dependency => dependency.Id).ToHashSet();

                var toDeleteIds = existingDependencies
                    .Where(ed => !currentDependencyIds.Contains(ed.DependentCommonAddonId)).Select(ed => (object)ed.Id).ToList();
                if (toDeleteIds.Count != 0)
                    db.DeleteAllIds<AddonDependency>(toDeleteIds);

                var existingDependencyIds = existingDependencies.Select(ed => ed.DependentCommonAddonId).ToHashSet();
                var toInsert = updatedAddon.Dependencies
                    .Where(d => !existingDependencyIds.Contains(d.Id))
                    .Select(d => new AddonDependency
                    {
                        CommonAddonId = updatedAddon.CommonAddonId,
                        DependentCommonAddonId = d.Id
                    }).ToList();
                if (toInsert.Count != 0)
                    db.InsertAll(toInsert);
            }
        }

        private static bool UpdateLocalAddonFieldsIfChanged(LocalAddon localAddon, ILocalAddon updatedAddon)
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

            if (localAddon.State != updatedAddon.State)
            {
                localAddon.State = updatedAddon.State;
                isChanged = true;
            }

            if (localAddon.InstallationMethod != updatedAddon.InstallationMethod)
            {
                localAddon.InstallationMethod = updatedAddon.InstallationMethod;
                isChanged = true;
            }

            return isChanged;
        }

        private static void RefreshLocalAddons(EsoDataConnection db, List<Addon> updatedAddons)
        {
            var localAddons = db.Table<LocalAddon>().ToList();
            var toUpdate = new List<LocalAddon>();

            foreach (var addon in localAddons)
            {
                var updatedAddon = updatedAddons.FirstOrDefault(a => a.CommonAddonId == addon.CommonAddonId);
                if (updatedAddon != null)
                {
                    if (UpdateLocalAddonFieldsIfChanged(addon, updatedAddon))
                        toUpdate.Add(addon);
                }
                else
                    db.Delete(addon);
            }

            var localCommonAddonIds = new HashSet<int>(localAddons.Select(localAddon => localAddon.CommonAddonId));
            var toInsert = (from updatedAddon in updatedAddons
                where !localCommonAddonIds.Contains(updatedAddon.CommonAddonId)
                select new LocalAddon
                {
                    CommonAddonId = updatedAddon.CommonAddonId,
                    Version = updatedAddon.Version,
                    DisplayedVersion = updatedAddon.DisplayedVersion,
                    State = updatedAddon.State,
                    InstallationMethod = updatedAddon.InstallationMethod
                }).ToList();

            if (toUpdate.Count != 0)
                db.UpdateAll(toUpdate);

            if (toInsert.Count != 0)
                db.InsertAll(toInsert);
        }

        private static bool UpdateOnlineAddonFieldsIfChanged(OnlineAddon onlineAddon, IOnlineAddon updatedAddon)
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

        private static void RefreshOnlineAddons(EsoDataConnection db, List<Addon> updatedAddons)
        {
            var onlineAddons = db.Table<OnlineAddon>().ToList();
            var toUpdate = new List<OnlineAddon>();

            foreach (var addon in onlineAddons)
            {
                var updatedAddon = updatedAddons.FirstOrDefault(a => a.CommonAddonId == addon.CommonAddonId);
                if (updatedAddon != null)
                {
                    if (UpdateOnlineAddonFieldsIfChanged(addon, updatedAddon))
                        toUpdate.Add(addon);
                }
                else
                    db.Delete(addon);
            }

            var onlineCommonAddonIds = new HashSet<int>(onlineAddons.Select(onlineAddon => onlineAddon.CommonAddonId));
            var toInsert = (from updatedAddon in updatedAddons
                where !onlineCommonAddonIds.Contains(updatedAddon.CommonAddonId)
                select new OnlineAddon
                {
                    CommonAddonId = updatedAddon.CommonAddonId,
                    LatestVersion = updatedAddon.LatestVersion,
                    DisplayedLatestVersion = updatedAddon.DisplayedLatestVersion,
                    UniqueId = updatedAddon.UniqueId
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
                    db.Delete(addonAuthor);

                var addonCategories = db.Table<AddonCategory>().Where(addonCategory => addonCategory.CommonAddonId == commonAddon.Id).ToList();
                foreach (var addonCategory in addonCategories)
                    db.Delete(addonCategory);

                var addonDependencies = db.Table<AddonDependency>()
                    .Where(addonDependency => addonDependency.CommonAddonId == commonAddon.Id).ToList();
                foreach (var addonDependency in addonDependencies)
                    db.Delete(addonDependency);

                db.Delete(commonAddon);
            }
        }

        private static void InsertOrUpdateAuthors(EsoDataConnection db, List<Addon> updatedAddons)
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

        private static void InsertOrUpdateCategories(EsoDataConnection db, List<Addon> updatedAddons)
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

        public static void InsertOrUpdateLocalAddon(EsoDataConnection db, Addon updatedAddon) =>
            InsertOrUpdateLocalAddons(db, [updatedAddon]);

        public static void InsertOrUpdateLocalAddons(EsoDataConnection db, IList<Addon> updatedAddons)
        {
            try
            {
                db.BeginTransaction();

                foreach (var addon in updatedAddons)
                {
                    var existingLocalAddonId = db.Table<LocalAddon>()
                        .Where(localAddon => localAddon.CommonAddonId == addon.CommonAddonId)
                        .Select(localAddon => localAddon.Id)
                        .FirstOrDefault();

                    var localAddon = addon.ToLocalAddon();
                    localAddon.Id = existingLocalAddonId;
                    if (existingLocalAddonId != null)
                        db.Update(localAddon);
                    else
                        db.Insert(localAddon);

                    if (!InstalledAddons.Contains(addon))
                        InstalledAddons.Add(addon);
                }

                db.Commit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                db.Rollback();
            }
        }

        public static void RemoveLocalAddon(EsoDataConnection db, Addon updatedAddon) =>
            RemoveLocalAddons(db, [updatedAddon]);

        public static void RemoveLocalAddons(EsoDataConnection db, IList<Addon> updatedAddons)
        {
            var commonAddonIds = updatedAddons.Select(addon => addon.CommonAddonId).Distinct().ToList();
            var toDelete = db.Table<LocalAddon>()
                .Where(localAddon => commonAddonIds.Contains(localAddon.CommonAddonId))
                .Select(localAddon => new LocalAddon { Id = localAddon.Id })
                .ToList();
            db.DeleteAll(toDelete);

            InstalledAddons.RemoveAll(addon => commonAddonIds.Contains(addon.CommonAddonId));
        }
    }
}
