using SpellCrafter.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpellCrafter.Services
{
    public static partial class AddonManifestParser
    {
        [GeneratedRegex(@"## Author:?\s*(.+)")]
        private static partial Regex AuthorRegex();

        [GeneratedRegex(@"## Version:?\s*(.+)")]
        private static partial Regex DisplayVersionRegex();

        [GeneratedRegex(@"## AddOnVersion:?\s*(\d+)")]
        private static partial Regex VersionRegex();

        [GeneratedRegex(@"## DependsOn:?\s*( [^>]+(>=\d+)?)+")]
        private static partial Regex DependsOnRegex();

        [GeneratedRegex(@"## Title:?\s*(.+)")]
        private static partial Regex TitleRegex();

        [GeneratedRegex(@"## Description:?\s*(.+)")]
        private static partial Regex DescriptionRegex();

        [GeneratedRegex(@"\|[cC][A-Fa-f0-9]{6}|(\|[rR])")]
        private static partial Regex ColorCodeRegex();

        [GeneratedRegex(@"\[[^\]]*\]|\([^\)]*\)")]
        private static partial Regex BracketsRegex();

        [GeneratedRegex(@"(?<=\s|^)@")]
        private static partial Regex AtSymbolRegex();

        public static Addon ParseAddonManifest(string manifestPath, bool isOnline)
        {
            var addon = new Addon();
            var lines = File.ReadAllLines(manifestPath);
            
            foreach (var line in lines)
            {
                var authorMatch = AuthorRegex().Match(line);
                if (authorMatch.Success)
                {
                    var authors = ParseAuthors(authorMatch.Groups[1].Value.Trim());
                    addon.Authors = new ObservableCollection<Author>(authors);
                }

                var displayVersionMatch = DisplayVersionRegex().Match(line);
                if (displayVersionMatch.Success)
                {
                    if (isOnline)
                        addon.DisplayedLatestVersion = displayVersionMatch.Groups[1].Value.Trim();
                    
                    addon.DisplayedVersion = displayVersionMatch.Groups[1].Value.Trim();
                    continue;
                }

                var versionMatch = VersionRegex().Match(line);
                if (versionMatch.Success)
                {
                    if (isOnline)
                        addon.LatestVersion = versionMatch.Groups[1].Value.Trim();

                    addon.Version = versionMatch.Groups[1].Value.Trim();
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
                        if (isOnline)
                            addon.OnlineDependencies.Add(dependency);
                        else
                            addon.LocalDependencies.Add(dependency);
                    }

                    continue;
                }

                var titleMatch = TitleRegex().Match(line);
                if (titleMatch.Success)
                {
                    var addonName = titleMatch.Groups[1].Value.Trim();
                    addon.Title = ColorCodeRegex().Replace(addonName, "");
                }

                var descriptionMatch = DescriptionRegex().Match(line);
                if (descriptionMatch.Success)
                {
                    var description = descriptionMatch.Groups[1].Value.Trim();
                    addon.Description = ColorCodeRegex().Replace(description, "");
                    continue;
                }
            }

            if (string.IsNullOrEmpty(addon.DisplayedVersion) && !string.IsNullOrEmpty(addon.Version))
                addon.DisplayedVersion = addon.Version;

            if (string.IsNullOrEmpty(addon.Version) && !string.IsNullOrEmpty(addon.DisplayedVersion))
                addon.Version = addon.DisplayedVersion;

            if (string.IsNullOrEmpty(addon.DisplayedLatestVersion) && !string.IsNullOrEmpty(addon.LatestVersion))
                addon.DisplayedLatestVersion = addon.LatestVersion;

            if (string.IsNullOrEmpty(addon.LatestVersion) && !string.IsNullOrEmpty(addon.DisplayedLatestVersion))
                addon.LatestVersion = addon.DisplayedLatestVersion;

            addon.Name = Path.GetFileNameWithoutExtension(manifestPath);
            if (string.IsNullOrEmpty(addon.Title))
                addon.Title = addon.Name;

            return addon;
        }

        private static List<Author> ParseAuthors(string authorsString)
        {
            var cleanedInput = ColorCodeRegex().Replace(authorsString, "");
            cleanedInput = BracketsRegex().Replace(cleanedInput, "");
            cleanedInput = AtSymbolRegex().Replace(cleanedInput, "");

            string[] separators = [",", "&", "/", "\\", "|", " and ", "et al."];
            var names = cleanedInput.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                .Select(name => name.Trim())
                .Where(name => !string.IsNullOrEmpty(name));

            return names.Select(name => new Author { Name = name.ToLower() }).ToList();
        }
    }
}
