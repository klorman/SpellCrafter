using Avalonia.Platform.Storage;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpellCrafter.Services;
using System.Diagnostics;
using System.IO;
using System;
using System.Reactive.Linq;
using Splat;

namespace SpellCrafter.ViewModels
{
    public class SettingsViewModel : ViewModelBase, IRoutableViewModel
    {
        const string AddonsDirectoryName = "AddOns";
        public string? UrlPathSegment => "/settings";
        public IScreen HostScreen { get; }

        [Reactive] public string AddonsDirectory { get; set; } = "";

        public RelayCommand BrowseAddonsFolderCommand { get; }
        public RelayCommand ApplyCommand { get; }

        public SettingsViewModel(IScreen? screen = null) : base()
        {
            HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;
            AddonsDirectory = AppSettings.Instance.AddonsDirectory;

            BrowseAddonsFolderCommand = new RelayCommand(_ => BrowseAddonsFolder());
            ApplyCommand = new RelayCommand(
                _ => Apply(),
                _ => !string.IsNullOrEmpty(AddonsDirectory)
            );

            this.WhenAnyValue(x => x.AddonsDirectory)
                .Subscribe(_ => ApplyCommand.RaiseCanExecuteChanged());
        }

        private async void BrowseAddonsFolder()
        {
            var options = new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                Title = "Addons folder"
            };

            var folderPath = await StorageProviderService.OpenFolderPickerAsync(options);
            var folderName = Path.GetFileName(folderPath);
            Debug.WriteLine(folderPath, folderName);

            if (string.IsNullOrEmpty(folderName) || !folderName.Equals(AddonsDirectoryName, StringComparison.OrdinalIgnoreCase))
                return;

            AddonsDirectory = folderPath;
        }

        private void Apply()
        {
            Debug.WriteLine("Apply!");
            if (AppSettings.Instance.AddonsDirectory != AddonsDirectory)
            {
                AppSettings.Instance.AddonsDirectory = AddonsDirectory;
                AppSettings.Instance.Save();
                AddonsScannerService.ScanAndUpdateDatabase(AddonsDirectory);
            }
        }
    }
}
