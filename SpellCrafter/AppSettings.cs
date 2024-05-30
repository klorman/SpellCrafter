using System.IO;
using Newtonsoft.Json;
using ReactiveUI.Fody.Helpers;

namespace SpellCrafter
{
    public class AppSettings
    {
        private const string SettingsPath = "settings.json";

        private static AppSettings? _instance;
        public static AppSettings Instance => _instance ?? (_instance = Load());

        [Reactive] public string AddonsDirectory { get; set; } = string.Empty;

        private AppSettings() { }

        private static AppSettings Load()
        {
            var filePath = SettingsPath;
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
            return new AppSettings();
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText(SettingsPath, json);
        }
    }
}
