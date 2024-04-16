using IniParser;
using IniParser.Model;
using SpellCrafter.Enums;
using System;
using System.IO;

namespace SpellCrafter
{
    public static class IniParser
    {
#if WINDOWS
        static readonly string IniFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/SpellCrafter/SpellCrafter.ini";
#else
        static readonly string IniFilePath = "~/.config/SpellCrafter/SpellCrafter.ini";
#endif
        const string Section = "Application";

        public static void InitIfNotExists()
        {
            var directoryPath = Path.GetDirectoryName(IniFilePath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath!);
            }

            if (!File.Exists(IniFilePath))
            {
                var parser = new FileIniDataParser();
                IniData data = new IniData();
                data[Section][IniDefines.AddonsFolderPath.ToString()] = "";

                parser.WriteFile(IniFilePath, data);
            }
        }

        public static string GetParam(IniDefines paramId, string defaultValue = "")
        {
            InitIfNotExists();
            var parser = new FileIniDataParser();
            var iniData = parser.ReadFile(IniFilePath);
            var res = iniData[Section][paramId.ToString()];
            return res;
        }

        public static void SetParam(IniDefines paramId, object value)
        {
            InitIfNotExists();
            var parser = new FileIniDataParser();
            var iniData = parser.ReadFile(IniFilePath);
            iniData[Section][paramId.ToString()] = value.ToString();
            parser.WriteFile(IniFilePath, iniData);
        }
    }
}
