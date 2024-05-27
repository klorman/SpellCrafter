using LinqToDB.Data;
using LinqToDB;
using SpellCrafter.Data;
using SpellCrafter.Enums;
using SpellCrafter.Models;
using SpellCrafter.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using DynamicData;

namespace SpellCrafter.Services
{
    public static class AddonsScannerService
    {
        public static void ScanAndUpdateDatabase(string path)
        {
            var addons = ScanDirectory(path);
            SynchronizeAddonsWithDatabase(addons);
        }

        private static List<Addon> ScanDirectory(string path)
        {
            var addons = new List<Addon>();

            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return addons;

            foreach (var addonDir in Directory.GetDirectories(path))
            {
                var addonName = Path.GetFileName(addonDir);
                var addonManifest = Path.Combine(addonDir, $"{addonName}.txt");

                if (!File.Exists(addonManifest))
                    continue;

                var addon = ParseAddonManifest(addonManifest);
                addon.Name = addonName;
                addon.AddonState = AddonState.LatestVersion;

                if (string.IsNullOrEmpty(addon.DisplayedVersion) && !string.IsNullOrEmpty(addon.Version))
                {
                    addon.DisplayedVersion = addon.Version;
                }

                addons.Add(addon);
            }

            return addons;
        }

        private static Addon ParseAddonManifest(string addonManifest)
        {
            var addon = new Addon();

            foreach (var line in File.ReadAllLines(addonManifest))
            {
                if (line.StartsWith("##"))
                {
                    var parts = line.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim().Substring(3);
                        var value = parts[1].Trim();

                        switch (key)
                        {
                            case "Author":
                                var authors = ParseAuthors(value);
                                addon.Authors.AddRange(authors);
                                break;
                            case "Version":
                                addon.DisplayedVersion = value;
                                break;
                            case "AddOnVersion":
                                addon.Version = value;
                                break;
                            case "Description":
                                addon.Description = value;
                                break;
                        }
                    }
                }
            }

            return addon;
        }

        private static List<Author> ParseAuthors(string authorsString)
        {
            var cleanedInput = Regex.Replace(authorsString, @"\|c[A-Fa-f0-9]{6}(@)?|\|r", "");

            string[] separators = [",", "&", "et al."];
            var names = cleanedInput.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(name => name.Trim())
                                    .Where(name => !string.IsNullOrEmpty(name));

            var authors = new List<Author>();
            foreach (var name in names)
            {
                authors.Add(new Author() { Name = name });
            }

            return authors;
        }

        private static void SynchronizeAddonsWithDatabase(List<Addon> scannedAddons)
        {
            using (var db = new ESODataConnection())
            {
                UpdateCommonAddons(scannedAddons, db);
                UpdateLocalAddons(scannedAddons, db);
                UpdateAuthors(scannedAddons, db);
                CleanupUnusedAddons(scannedAddons, db);
            }
        }

        private static void UpdateCommonAddons(List<Addon> scannedAddons, ESODataConnection db)
        {
            var existingCommonAddons = db.CommonAddons.ToDictionary(a => a.Name);
            var commonAddonsToUpdate = new List<CommonAddon>();

            foreach (var scannedAddon in scannedAddons)
            {
                if (existingCommonAddons.TryGetValue(scannedAddon.Name, out var existingAddon))
                {
                    scannedAddon.CommonAddonId = existingAddon.Id;
                    if (existingAddon.Description != scannedAddon.Description)
                    {
                        existingAddon.Description = scannedAddon.Description;
                        commonAddonsToUpdate.Add(existingAddon);
                    }
                }
                else
                {
                    var newAddon = new CommonAddon { Name = scannedAddon.Name, Description = scannedAddon.Description };
                    var insertedAddonId = Convert.ToInt32(db.InsertWithIdentity(newAddon));
                    scannedAddon.CommonAddonId = insertedAddonId;
                }
            }

            if (commonAddonsToUpdate.Any())
            {
                db.Update(commonAddonsToUpdate);
            }
        }

        private static void UpdateLocalAddons(List<Addon> scannedAddons, ESODataConnection db)
        {
            var currentLocalAddons = db.LocalAddons.ToList();
            var localAddonsToUpdate = new List<LocalAddon>();
            var localAddonsToInsert = new List<LocalAddon>();

            var currentLocalAddonsDict = currentLocalAddons.ToDictionary(a => a.CommonAddonId);

            foreach (var scannedAddon in scannedAddons)
            {
                if (currentLocalAddonsDict.TryGetValue(scannedAddon.CommonAddonId, out var existingAddon))
                {
                    if (existingAddon.Version != scannedAddon.Version ||
                        existingAddon.DisplayedVersion != scannedAddon.DisplayedVersion ||
                        existingAddon.State != scannedAddon.AddonState ||
                        existingAddon.InstallationMethod != scannedAddon.InstallationMethod)
                    {
                        existingAddon.Version = scannedAddon.Version;
                        existingAddon.DisplayedVersion = scannedAddon.DisplayedVersion;
                        existingAddon.State = scannedAddon.AddonState;
                        existingAddon.InstallationMethod = scannedAddon.InstallationMethod;
                        localAddonsToUpdate.Add(existingAddon);
                    }
                }
                else
                {
                    localAddonsToInsert.Add(new LocalAddon
                    {
                        CommonAddonId = scannedAddon.CommonAddonId,
                        Version = scannedAddon.Version,
                        DisplayedVersion = scannedAddon.DisplayedVersion,
                        State = scannedAddon.AddonState,
                        InstallationMethod = scannedAddon.InstallationMethod
                    });
                }
            }

            if (localAddonsToUpdate.Any())
                db.Update(localAddonsToUpdate);

            if (localAddonsToInsert.Any())
                db.BulkCopy(new BulkCopyOptions(), localAddonsToInsert);
        }

        private static void UpdateAuthors(List<Addon> scannedAddons, ESODataConnection db)
        {
            var existingAuthors = db.Authors.ToDictionary(a => a.Name.ToLower());

            foreach (var scannedAddon in scannedAddons)
            {
                foreach (var scannedAuthor in scannedAddon.Authors)
                {
                    var lowerAuthorName = scannedAuthor.Name.ToLower();
                    if (!existingAuthors.TryGetValue(lowerAuthorName, out var existingAuthor))
                    {
                        var newAuthor = new Author { Name = lowerAuthorName };
                        newAuthor.Id = Convert.ToInt32(db.InsertWithIdentity(newAuthor));
                        existingAuthors[lowerAuthorName] = newAuthor;
                    }
                }
            }

            UpdateAddonAuthors(scannedAddons, existingAuthors, db);
            CleanupUnusedAuthors(db);
        }

        private static void UpdateAddonAuthors(List<Addon> scannedAddons, Dictionary<string, Author> existingAuthors, ESODataConnection db)
        {
            var currentAddonAuthors = db.AddonAuthors.ToList();
            var addonAuthorsToUpdate = new List<AddonAuthor>();

            foreach (var scannedAddon in scannedAddons)
            {
                var currentAuthors = currentAddonAuthors.Where(a => a.CommonAddonId == scannedAddon.CommonAddonId).ToList();
                var scannedAuthorIds = scannedAddon.Authors.Select(a => existingAuthors[a.Name.ToLower()].Id).ToList();

                var authorsToRemove = currentAuthors.Where(a => !scannedAuthorIds.Contains(a.AuthorId)).ToList();
                if (authorsToRemove.Any())
                    db.AddonAuthors.Delete(a => authorsToRemove.Select(x => x.Id).Contains(a.Id));

                var currentAuthorIds = currentAuthors.Select(a => a.AuthorId).ToList();
                var authorsToAdd = scannedAuthorIds.Where(id => !currentAuthorIds.Contains(id))
                                                   .Select(id => new AddonAuthor { CommonAddonId = scannedAddon.CommonAddonId, AuthorId = id }).ToList();
                if (authorsToAdd.Any())
                    db.BulkCopy(new BulkCopyOptions(), authorsToAdd);
            }
        }

        private static void CleanupUnusedAuthors(ESODataConnection db)
        {
            var authorsWithLinks = db.AddonAuthors.Select(a => a.AuthorId).Distinct();
            var allAuthors = db.Authors.Select(a => a.Id);
            var authorsToDelete = allAuthors.Except(authorsWithLinks).ToList();

            if (authorsToDelete.Any())
                db.Authors.Delete(a => authorsToDelete.Contains(a.Id));
        }

        private static void CleanupUnusedAddons(List<Addon> scannedAddons, ESODataConnection db)
        {
            var localAddonsToDelete = db.LocalAddons.Where(a => !scannedAddons.Any(sa => sa.CommonAddonId == a.CommonAddonId)).ToList();
            var localAddonIdsToDelete = localAddonsToDelete.Select(a => a.CommonAddonId).ToList();

            if (localAddonsToDelete.Any())
            {
                db.LocalAddons.Delete(a => localAddonIdsToDelete.Contains(a.CommonAddonId));

                var currentOnlineAddons = db.OnlineAddons.ToList();
                var onlineAddonIds = currentOnlineAddons.Select(o => o.CommonAddonId).Distinct().ToList();
                var commonAddonIdsToDelete = localAddonIdsToDelete.Where(id => !onlineAddonIds.Contains(id)).ToList();

                if (commonAddonIdsToDelete.Any())
                {
                    db.CommonAddons.Delete(a => commonAddonIdsToDelete.Contains(a.Id));
                }
            }
        }
    }
}
