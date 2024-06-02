using SpellCrafter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DynamicData;

namespace SpellCrafter.Services
{
    public static partial class AddonManifestParser
    {
        [GeneratedRegex(@"## Author: (.+)")]
        private static partial Regex AuthorRegex();

        [GeneratedRegex(@"## Version: (.+)")]
        private static partial Regex DisplayVersionRegex();

        [GeneratedRegex(@"## AddOnVersion: (\d+)")]
        private static partial Regex VersionRegex();

        [GeneratedRegex(@"## DependsOn:( [^>]+(>=\d+)?)+")]
        private static partial Regex DependsOnRegex();

        [GeneratedRegex(@"## Title: (.+)")]
        private static partial Regex TitleRegex();

        [GeneratedRegex(@"## Description: (.+)")]
        private static partial Regex DescriptionRegex();

        public static Addon ParseAddonManifest(string manifestPath)
        {
            var addon = new Addon();
            var lines = File.ReadAllLines(manifestPath);
            
            foreach (var line in lines)
            {
                var authorMatch = AuthorRegex().Match(line);
                if (authorMatch.Success)
                {
                    var authors = ParseAuthors(authorMatch.Groups[1].Value.Trim());
                    addon.Authors.AddRange(authors);
                }

                var displayVersionMatch = DisplayVersionRegex().Match(line);
                if (displayVersionMatch.Success)
                {
                    addon.DisplayedVersion = displayVersionMatch.Groups[1].Value.Trim();
                    continue;
                }

                var versionMatch = VersionRegex().Match(line);
                if (versionMatch.Success)
                {
                    addon.Version = versionMatch.Groups[1].Value;
                    continue;
                }

                var dependsOnMatch = DependsOnRegex().Match(line);
                if (dependsOnMatch.Success)
                {
                    var tokens = dependsOnMatch.Value.Split(' ')[2..];
                    foreach (var token in tokens)
                    {
                        var parts = token.Trim().Split(">=");
                        var dependency = new CommonAddon { Name = parts[0] }; // TODO add support for min version
                        addon.Dependencies.Add(dependency);
                    }

                    continue;
                }

                var titleMatch = TitleRegex().Match(line);
                if (titleMatch.Success)
                {
                    var addonName = titleMatch.Groups[1].Value.Trim();
                    addon.Name = ColorCodeRegex().Replace(addonName, "");
                }

                var descriptionMatch = DescriptionRegex().Match(line);
                if (descriptionMatch.Success)
                {
                    addon.Description = descriptionMatch.Groups[1].Value.Trim();
                    continue;
                }
            }

            if (string.IsNullOrEmpty(addon.DisplayedVersion) && !string.IsNullOrEmpty(addon.Version))
            {
                addon.DisplayedVersion = addon.Version;
            }

            if (string.IsNullOrEmpty(addon.Version) && !string.IsNullOrEmpty(addon.DisplayedVersion))
            {
                addon.Version = addon.DisplayedVersion;
            }

            return addon;
        }

        [GeneratedRegex(@"\|c[A-Fa-f0-9]{6}(@)?|\|r")]
        private static partial Regex ColorCodeRegex();

        private static List<Author> ParseAuthors(string authorsString)
        {
            var cleanedInput = ColorCodeRegex().Replace(authorsString, "");

            string[] separators = [",", "&", "et al."];
            var names = cleanedInput.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                .Select(name => name.Trim())
                .Where(name => !string.IsNullOrEmpty(name));

            return names.Select(name => new Author { Name = name }).ToList();
        }
    }
}
