using SpellCrafter.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpellCrafter.Services
{
    public class AddonVersionComparer : IComparer<string?>
    {
        public int Compare(string? version1, string? version2) =>
            CompareVersions(version1, version2);

        public static int CompareVersions(Addon addon1, Addon addon2) =>
            CompareVersions(addon1.Version, addon2.Version);

        private static readonly char[] Separator = ['.'];

        public static int CompareVersions(string? version1, string? version2)
        {
            if (version1 == null && version2 == null) return 0;
            if (version1 == null) return -1;
            if (version2 == null) return 1;

            var parts1 = GetVersionParts(version1);
            var parts2 = GetVersionParts(version2);

            var maxLength = Math.Max(parts1.Count, parts2.Count);
            for (var i = 0; i < maxLength; ++i)
            {
                var part1 = i < parts1.Count ? parts1[i] : string.Empty;
                var part2 = i < parts2.Count ? parts2[i] : string.Empty;

                var result = int.TryParse(part1, out var num1) && int.TryParse(part2, out var num2)
                    ? num1.CompareTo(num2)
                    : string.Compare(part1, part2, StringComparison.Ordinal);

                if (result != 0)
                    return result;
            }

            return 0;

            static List<string> GetVersionParts(string version) =>
                version.Split(Separator, StringSplitOptions.RemoveEmptyEntries)
                    .Select(part => part.TrimStart('0'))
                    .ToList();
        }
    }
}
