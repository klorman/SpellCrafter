using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpellCrafter.Enums;
using System.Diagnostics;
using System.Windows.Input;

namespace SpellCrafter.ViewModels
{
    public class Addon : ViewModelBase
    {
        [Reactive] public long Id { get; set; }
        [Reactive] public string Name { get; set; } = "";
        [Reactive] public string ArchiveName { get; set; } = "";
        [Reactive] public AddonState AddonState { get; set; } = AddonState.NotInstalled;
        [Reactive] public int Downloads { get; set; } = 0;
        [Reactive] public string Category { get; set; } = "";
        [Reactive] public string Latest { get; set; } = "";
        [Reactive] public string GameVersion { get; set; } = "";
        [Reactive] public string Author { get; set; } = "";
        [Reactive] public string Link { get; set; } = "";
        [Reactive] public string Description { get; set; } = "";
        [Reactive] public string FileSize { get; set; } = "";
        [Reactive] public string Overview { get; set; } = "";

        public ICommand InstallCommand { get; }
        public ICommand ReinstallCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ViewWebsiteCommand { get; }
        public ICommand CopyLinkCommand { get; }
        public ICommand BrowseFolderCommand { get; }

        public Addon() : base()
        {
            InstallCommand = new RelayCommand(
                _ => Install(), 
                _ => AddonState == AddonState.NotInstalled
            );
            ReinstallCommand = new RelayCommand(
                _ => Reinstall(),
                _ => AddonState != AddonState.NotInstalled
            );
            UpdateCommand = new RelayCommand(
                _ => Update(),
                _ => AddonState == AddonState.Outdated || AddonState == AddonState.UpdateError
            );
            DeleteCommand = new RelayCommand(
                _ => Delete(),
                _ => AddonState != AddonState.NotInstalled
            );
            ViewWebsiteCommand = new RelayCommand(_ => ViewWebsite());
            CopyLinkCommand = new RelayCommand(_ => CopyLink());
            BrowseFolderCommand = new RelayCommand(
                _ => BrowseFolder(), 
                _ => AddonState != AddonState.NotInstalled
            );
        }

        private void Install()
        {
            Debug.WriteLine($"InstallMod! {Name}");
        }

        private void Reinstall()
        {
            Debug.WriteLine($"Reinstall! {Name}");
        }

        private void Update()
        {
            Debug.WriteLine("Update!");
        }

        private void Delete()
        {
            Debug.WriteLine("Delete!");
        }

        private void ViewWebsite()
        {
            Debug.WriteLine("ViewWebsite!");
        }

        private void CopyLink()
        {
            Debug.WriteLine("CopyLink!");
        }

        private void BrowseFolder()
        {
            Debug.WriteLine("BrowseFolder!");
        }
    }
}
