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

            if (string.IsNullOrEmpty(path))
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
                                var author = new Author()
                                {
                                    Name = value,
                                };
                                addon.Authors.Add(author); // TODO сделать парсинг строки авторов
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

        private static void SynchronizeAddonsWithDatabase(List<Addon> scannedAddons)
        {
            using (var db = new ESODataConnection())
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
                        var newAddon = new CommonAddon
                        {
                            Name = scannedAddon.Name,
                            Description = scannedAddon.Description
                        };
                        var insertedAddonId = Convert.ToInt32(db.InsertWithIdentity(newAddon));
                        scannedAddon.CommonAddonId = insertedAddonId;
                    }
                }

                if (commonAddonsToUpdate.Any())
                {
                    db.Update(commonAddonsToUpdate);
                }

                var currentLocalAddons = db.LocalAddons.ToList();
                var currentLocalAddonsDict = currentLocalAddons.ToDictionary(a => a.CommonAddonId);

                var localAddonsToUpdate = new List<LocalAddon>();
                var localAddonsToInsert = new List<LocalAddon>();

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
                        var newAddon = new LocalAddon
                        {
                            CommonAddonId = scannedAddon.CommonAddonId,
                            Version = scannedAddon.Version,
                            DisplayedVersion = scannedAddon.DisplayedVersion,
                            State = scannedAddon.AddonState,
                            InstallationMethod = scannedAddon.InstallationMethod,
                        };
                        localAddonsToInsert.Add(newAddon);
                    }
                }

                var localAddonsToDelete = currentLocalAddons.Where(a => !scannedAddons.Any(sa => sa.CommonAddonId == a.CommonAddonId)).ToList();

                if (localAddonsToUpdate.Any())
                    db.Update(localAddonsToUpdate);

                if (localAddonsToInsert.Any())
                    db.BulkCopy(new BulkCopyOptions(), localAddonsToInsert);

                if (localAddonsToDelete.Any())
                {
                    var localAddonIdsToDelete = localAddonsToDelete.Select(a => a.CommonAddonId).ToList();
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
}
