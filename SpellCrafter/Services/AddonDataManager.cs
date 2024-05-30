using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SpellCrafter.Data;
using SpellCrafter.Models;
using SQLiteNetExtensions.Extensions;

namespace SpellCrafter.Services
{
    public static class AddonDataManager
    {
        public static void UpdateLocalAddonList(EsoDataConnection db, List<Addon> addons)
        {
            UpdateCommonAddons(db, addons);
            UpdateLocalAddons(db, addons);
            RemoveUnusedCommonAddons(db);

            UpdateAuthors(db, addons);
            RemoveUnusedAuthors(db);
            UpdateCategories(db, addons);
            RemoveUnusedCategories(db);
        }

        public static void UpdateOnlineAddonList(EsoDataConnection db, List<OnlineAddon> onlineAddons)
        {
            throw new NotImplementedException();
        }

        public static List<Addon> GetAllLocalAddonsWithDetails(EsoDataConnection db)
        {
            var localAddons = db.GetAllWithChildren<LocalAddon>(recursive: true);

            var addons = localAddons.Select(la => new Addon
            {
                CommonAddonId = la.CommonAddonId,
                Name = la.CommonAddon.Name,
                Description = la.CommonAddon.Description,
                AddonState = la.State,
                InstallationMethod = la.InstallationMethod,
                Categories = new ObservableCollection<Category>(la.CommonAddon.Categories),
                Authors = new ObservableCollection<Author>(la.CommonAddon.Authors),
                UniqueIdentifier = (la.CommonAddon.OnlineAddon != null)
                    ? la.CommonAddon.OnlineAddon.UniqueIdentifier
                    : string.Empty,
                Version = la.Version,
                DisplayedVersion = la.DisplayedVersion,
                LatestVersion = (la.CommonAddon.OnlineAddon != null)
                    ? la.CommonAddon.OnlineAddon.LatestVersion
                    : string.Empty,
                DisplayedLatestVersion = (la.CommonAddon.OnlineAddon != null)
                    ? la.CommonAddon.OnlineAddon.DisplayedLatestVersion
                    : string.Empty,
                Dependencies = la.CommonAddon.Dependencies
            }).ToList();

            return addons;
        }
            
        private static void UpdateCommonAddons(EsoDataConnection db, List<Addon> updatedAddons)
        {
            if (updatedAddons.Count == 0)
                return;

            var commonAddons = db.Table<CommonAddon>().ToList().ToDictionary(a => a.Name, a => a);
            var toInsert = new List<CommonAddon>();
            var toUpdate = new List<CommonAddon>();

            foreach (var addon in updatedAddons)
            {
                if (!commonAddons.TryGetValue(addon.Name, out var dbAddon))
                {
                    dbAddon = new CommonAddon
                    {
                        Name = addon.Name,
                        Description = addon.Description
                        // TODO Add dependencies
                    };
                    toInsert.Add(dbAddon);
                }
                else
                {
                    addon.CommonAddonId = dbAddon.Id;
                    if (dbAddon.Description == addon.Description) continue;
                    dbAddon.Description = addon.Description;
                    // TODO update dependencies
                    toUpdate.Add(dbAddon);
                }
            }

            if (toInsert.Count != 0)
            {
                db.InsertAll(toInsert);

                foreach (var addon in toInsert)
                {
                    var updateAddon = updatedAddons.First(a => a.Name == addon.Name);
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
                {
                    db.Delete(addon);
                }
            }

            var localCommonAddonIds = new HashSet<int>(localAddons.Select(a => a.CommonAddonId));
            var toInsert = (from updatedAddon in updatedAddons
                where !localCommonAddonIds.Contains(updatedAddon.CommonAddonId)
                select new LocalAddon
                {
                    CommonAddonId = updatedAddon.CommonAddonId,
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

        private static void RemoveUnusedCommonAddons(EsoDataConnection db)
        {
            var usedCommonAddonIds = new HashSet<int>(
                db.Table<LocalAddon>().Select(la => la.CommonAddonId)
                    .Union(db.Table<OnlineAddon>().Select(oa => oa.CommonAddonId))
            );

            var unusedCommonAddons = db.Table<CommonAddon>().Where(ca => !usedCommonAddonIds.Contains(ca.Id));
            foreach (var commonAddon in unusedCommonAddons)
            {
                db.Delete(commonAddon);
            }
        }

        private static void UpdateAuthors(EsoDataConnection db, List<Addon> updatedAddons)
        {
            var authors = db.GetAllWithChildren<Author>().ToList().ToDictionary(a => a.Name, a => a);

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
            var categories = db.GetAllWithChildren<Category>().ToList().ToDictionary(c => c.Name, c => c);

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
