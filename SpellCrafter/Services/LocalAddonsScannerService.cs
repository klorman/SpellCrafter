using SpellCrafter.Enums;
using SpellCrafter.Models;
using System.Collections.Generic;
using System.IO;

namespace SpellCrafter.Services
{
    public static class LocalAddonsScannerService
    {
        public static List<Addon> ScanDirectory(string path)
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

                var addon = AddonManifestParser.ParseAddonManifest(addonManifest, false);
                addon.State = AddonState.LatestVersion; // TODO check latest version

                addons.Add(addon);
            }

            return addons;
        }
    }
}
