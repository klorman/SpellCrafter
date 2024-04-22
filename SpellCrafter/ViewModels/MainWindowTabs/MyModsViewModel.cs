using ReactiveUI;
using SpellCrafter.Enums;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;

namespace SpellCrafter.ViewModels.MainWindowTabs
{
    public class MyModsViewModel : AddonsOverviewViewModel, IActivatableViewModel
    {
        private bool isInitialized = false;
        public ViewModelActivator Activator { get; } = new();

        public MyModsViewModel() : base(false)
        {
            this.WhenActivated((CompositeDisposable disposable) =>
            {
                if (!isInitialized)
                {
                    LoadLocalAddons();
                    isInitialized = true;
                }
            });
        }

        public async void LoadLocalAddons()
        {
            var addonsFolderPath = IniParser.GetParam(IniDefines.AddonsFolderPath);

            if (string.IsNullOrEmpty(addonsFolderPath))
            {
                Debug.WriteLine("AddonsFolderPath is empty!");
                await ShowMainDialogAsync(new AddonFolderSelectionDialogViewModel());
                addonsFolderPath = IniParser.GetParam(IniDefines.AddonsFolderPath);
            }

            var addons = new List<Addon>();

            foreach (var addonDir in Directory.GetDirectories(addonsFolderPath))
            {
                var addonName = Path.GetFileName(addonDir);
                var addonFile = Path.Combine(addonDir, $"{addonName}.txt");
                if (File.Exists(addonFile))
                {
                    var addon = new Addon
                    {
                        Name = addonName,
                        AddonState = AddonState.LatestVersion
                    };
                    foreach (var line in File.ReadAllLines(addonFile))
                    {
                        if (line.StartsWith("##"))
                        {
                            var parts = line.Split(':', 2);
                            if (parts.Length == 2)
                            {
                                var key = parts[0].Trim().Substring(3);
                                var value = parts[1].Trim();
                                Debug.WriteLine($"\"{key}\"");
                                switch (key)
                                {
                                    case "Author":
                                        Debug.WriteLine($"AUTHOR!!!! {value}");
                                        addon.Author = value;
                                        break;
                                    case "Version":
                                        addon.DisplayedVersion = value;
                                        break;
                                    case "AddOnVersion":
                                        addon.AddonVersion = value;
                                        break;
                                    case "Description":
                                        addon.Description = value;
                                        break;
                                }
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(addon.DisplayedVersion) && !string.IsNullOrEmpty(addon.AddonVersion))
                    {
                        addon.DisplayedVersion = addon.AddonVersion;
                    }

                    addons.Add(addon);
                }
            }

            modsSource.AddRange(addons);
            FilterMods();
        }
    }
}
