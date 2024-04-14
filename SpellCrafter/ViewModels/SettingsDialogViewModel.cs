using Avalonia.Platform.Storage;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpellCrafter.Services;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace SpellCrafter.ViewModels
{
    public class SettingsDialogViewModel : ReactiveObject
    {
        const string AddonsFolderName = "AddOns";

        [Reactive] public string GameInstancePath { get; set; } = "";

        public ICommand BrowseGameInstanceFolderCommand { get; }
        
        public SettingsDialogViewModel() : base()
        {
            BrowseGameInstanceFolderCommand = new RelayCommand(_ => BrowseGameInstanceFolder());
        }

        private async void BrowseGameInstanceFolder()
        {
            var options = new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                Title = "Укажите папку для сохранения файлов настройки"
            };

            var folderPath = await StorageProviderService.OpenFolderPickerAsync(options);
            var folderName = Path.GetFileName(folderPath);
            Debug.WriteLine(folderPath, folderName);

            if (string.IsNullOrEmpty(folderName) || !folderName.Equals(AddonsFolderName))
                return;

            GameInstancePath = folderPath;
        }
    }
}
