using IniParser;
using IniParser.Model;
using SpellCrafter.Enums;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace SpellCrafter
{
    public static class IniParser
    {
        private static IniData? _cachedIniData = null;

#if WINDOWS
        static readonly string IniFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/SpellCrafter/SpellCrafter.ini";
#else
        static readonly string IniFilePath = "~/.config/SpellCrafter/SpellCrafter.ini";
#endif
        const string Section = "Application";

        [MemberNotNull(nameof(_cachedIniData))]
        private static void LoadIniData()
        {
            if (_cachedIniData == null)
            {
                var directoryPath = Path.GetDirectoryName(IniFilePath);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath!);
                }

                var parser = new FileIniDataParser();

                if (!File.Exists(IniFilePath))
                {
                    IniData data = new IniData();
                    data[Section][IniDefines.AddonsFolderPath.ToString()] = "";

                    parser.WriteFile(IniFilePath, data);
                    _cachedIniData = data;
                }
                else
                {
                    _cachedIniData = parser.ReadFile(IniFilePath);
                }
            }
        }

        public static string GetParam(IniDefines paramId, string defaultValue = "")
        {
            LoadIniData();
            var res = _cachedIniData[Section][paramId.ToString()];
            return !string.IsNullOrEmpty(res) ? res : defaultValue;
        }

        public static void SetParam(IniDefines paramId, object value)
        {
            LoadIniData();
            _cachedIniData[Section][paramId.ToString()] = value.ToString();
            var parser = new FileIniDataParser();
            parser.WriteFile(IniFilePath, _cachedIniData);
        }
    }
}
