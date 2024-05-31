using System.IO;
using Newtonsoft.Json;
using ReactiveUI;

namespace SpellCrafter
{
    public class AppSettings : ReactiveObject
    {
        private const string SettingsPath = "settings.json";

        private static AppSettings? _instance;
        public static AppSettings Instance => _instance ??= Load();

        [JsonProperty(PropertyName = nameof(AddonsDirectory))]
        private string _addonsDirectory = string.Empty;

        public string AddonsDirectory
        {
            get => _addonsDirectory;
            set => Instance.RaiseAndSetIfChanged(ref _addonsDirectory, value);
        }

        private AppSettings() { }

        private static AppSettings Load()
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
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
