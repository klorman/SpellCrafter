using DynamicData;
using SpellCrafter.Enums;
using SpellCrafter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpellCrafter.Services
{
    public static partial class AddonsScannerService
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
                if (!line.StartsWith("##")) continue;
                var parts = line.Split(':', 2);
                if (parts.Length != 2) continue;

                var key = parts[0].Trim()[3..];
                var value = parts[1].Trim();

                switch (key) // TODO Parse Dependencies
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

            return addon;
        }

        [GeneratedRegex(@"\|c[A-Fa-f0-9]{6}(@)?|\|r")]
        private static partial Regex AuthorRegex();

        private static List<Author> ParseAuthors(string authorsString)
        {
            var cleanedInput = AuthorRegex().Replace(authorsString, "");

            string[] separators = [",", "&", "et al."];
            var names = cleanedInput.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                .Select(name => name.Trim())
                .Where(name => !string.IsNullOrEmpty(name));

            return names.Select(name => new Author() { Name = name }).ToList();
        }
    }
}
