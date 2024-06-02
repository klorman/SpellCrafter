using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpellCrafter.Enums;
using SpellCrafter.Messages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace SpellCrafter.Models
{
    public class Addon : ReactiveObject
    {
        private const string BaseDownloadLink = "https://www.esoui.com/downloads";
        
        public List<CommonAddon> Dependencies { get; set; } = [];
        public int? CommonAddonId { get; set; }
        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public string Description { get; set; } = string.Empty;
        [Reactive] public AddonState AddonState { get; set; } = AddonState.NotInstalled;
        [Reactive] public AddonInstallationMethod InstallationMethod { get; set; } = AddonInstallationMethod.Other;
        [Reactive] public string Downloads { get; set; } = string.Empty;
        [Reactive] public ObservableCollection<Category> Categories { get; set; } = [];
        [Reactive] public ObservableCollection<Author> Authors { get; set; } = [];
        [Reactive] public int? UniqueId { get; set; }
        [Reactive] public string FileSize { get; set; } = string.Empty;
        [Reactive] public string Overview { get; set; } = string.Empty;
        [Reactive] public string Version { get; set; } = string.Empty;
        [Reactive] public string DisplayedVersion { get; set; } = string.Empty;
        [Reactive] public string LatestVersion { get; set; } = string.Empty;
        [Reactive] public string DisplayedLatestVersion { get; set; } = string.Empty;
        [Reactive] public string GameVersion { get; set; } = string.Empty;

        public ICommand ViewModCommand { get; }
        public ICommand InstallCommand { get; }
        public ICommand ReinstallCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ViewWebsiteCommand { get; }
        public ICommand CopyLinkCommand { get; }
        public ICommand BrowseFolderCommand { get; }

        public Addon()
        {
            ViewModCommand = new RelayCommand(_ => ViewMod());
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

        private void ViewMod()
        {
            Debug.WriteLine("ViewMod!");

            MessageBus.Current.SendMessage(new ViewAddonMessage(this));
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
