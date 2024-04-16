using Avalonia.Platform.Storage;
using ReactiveUI.Fody.Helpers;
using SpellCrafter.Services;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace SpellCrafter.ViewModels
{
    public class AddonFolderSelectionDialogViewModel : ViewModelBase
    {
        const string AddonsFolderName = "AddOns";

        [Reactive] public string AddonsFolderPath { get; set; } = "";

        public ICommand BrowseAddonsFolderCommand { get; }
        public ICommand ApplyCommand { get; }

        public AddonFolderSelectionDialogViewModel() : base()
        {
            BrowseAddonsFolderCommand = new RelayCommand(_ => BrowseAddonsFolder());
            ApplyCommand = new RelayCommand(
                _ => Apply(),
                _ => string.IsNullOrEmpty(AddonsFolderPath)
            );
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

            if (string.IsNullOrEmpty(folderName) || !folderName.Equals(AddonsFolderName))
                return;

            AddonsFolderPath = folderPath;
        }

        private void Apply()
        {
            Debug.WriteLine("Apply!");
        }
    }
}
