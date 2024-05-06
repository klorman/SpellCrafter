using Avalonia.Platform.Storage;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpellCrafter.Data;
using SpellCrafter.Enums;
using SpellCrafter.Models;
using SpellCrafter.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System;
using System.Reactive.Linq;

namespace SpellCrafter.ViewModels
{
    public class SettingsDialogViewModel : ViewModelBase
    {
        const string AddonsDirectoryName = "AddOns";

        [Reactive] public string AddonsDirectory { get; set; } = "";

        public RelayCommand BrowseAddonsFolderCommand { get; }
        public RelayCommand ApplyCommand { get; }

        public SettingsDialogViewModel() : base()
        {
            BrowseAddonsFolderCommand = new RelayCommand(_ => BrowseAddonsFolder());
            ApplyCommand = new RelayCommand(
                _ => Apply(),
                _ => string.IsNullOrEmpty(AddonsDirectory)
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

            if (string.IsNullOrEmpty(folderName) || !folderName.Equals(AddonsDirectoryName))
                return;

            AddonsDirectory = folderPath;
        }

        private void Apply()
        {
            Debug.WriteLine("Apply!");
            AddonsScannerService.ScanAndUpdateDatabase(AddonsDirectoryName);
        }
    }
}
