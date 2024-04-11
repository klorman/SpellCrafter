using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpellCrafter.Enums;
using SpellCrafter.Models.DbClasses;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace SpellCrafter.ViewModels.MainWindowTabs
{
    public class AddonsOverviewViewModel : ReactiveObject
    {
        private readonly List<Addon> modsSource = new();
        [Reactive] public ObservableCollection<Addon> DisplayedMods { get; set; } = new();
        [Reactive] public string ModsFilter { get; set; } = "";
        [Reactive] public bool BrowseMode { get; set; }
        [Reactive] public Addon? DataGridModsSelectedItem { get; set; }

        public ICommand UpdateAllCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand InstallModsCommand { get; }
        public ICommand InstallModCommand { get; }
        public ICommand FilterModsCommand { get; }
        public ICommand RefreshModsCommand { get; }
        public ICommand ReinstallCommand { get; }
        public ICommand ViewModCommand { get; }
        public ICommand ViewModWebsiteCommand { get; }
        public ICommand CopyModLinkCommand { get; }
        public ICommand BrowseFolderCommand { get; }
        public ICommand DeleteCommand { get; }

        public AddonsOverviewViewModel(bool browseMode) : base()
        {
            BrowseMode = browseMode;

            modsSource.Add(new() { Name = "CustomCompassPins", ArchiveName = "CustomCompassPins.zip", AddonState = AddonState.NotInstalled, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21", Description = "Slight improvements to the default experience bar that adds current/max experience and always displays the bar" });
            modsSource.Add(new() { Name = "LibAddonMenu", ArchiveName = "LibAddonMenu.zip", AddonState = AddonState.LatestVersion, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21", Description = "Slight improvements to the default experience bar that adds current/max experience and always displays the bar" });
            modsSource.Add(new() { Name = "MiniMap by Fyrakin", ArchiveName = "MiniMap by Fyrakin.zip", AddonState = AddonState.LatestVersion, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21", Description = "Slight improvements to the default experience bar that adds current/max experience and always displays the bar" });
            modsSource.Add(new() { Name = "LibLazyCrafting", ArchiveName = "LibLazyCrafting.zip", AddonState = AddonState.LatestVersion, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21", Description = "Slight improvements to the default experience bar that adds current/max experience and always displays the bar" });
            modsSource.Add(new() { Name = "CustomCompassPins", ArchiveName = "CustomCompassPins.zip", AddonState = AddonState.LatestVersion, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21", Description = "Slight improvements to the default experience bar that adds current/max experience and always displays the bar" });
            modsSource.Add(new() { Name = "LibAddonMenu", ArchiveName = "LibAddonMenu.zip", AddonState = AddonState.LatestVersion, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21", Description = "Slight improvements to the default experience bar that adds current/max experience and always displays the bar" });
            modsSource.Add(new() { Name = "MiniMap by Fyrakin", ArchiveName = "MiniMap by Fyrakin.zip", AddonState = AddonState.LatestVersion, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21", Description = "Slight improvements to the default experience bar that adds current/max experience and always displays the bar" });
            modsSource.Add(new() { Name = "LibLazyCrafting", ArchiveName = "LibLazyCrafting.zip", AddonState = AddonState.LatestVersion, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21", Description = "Slight improvements to the default experience bar that adds current/max experience and always displays the bar" });

            FilterMods();

            UpdateAllCommand = new RelayCommand(_ => UpdateAll());
            UpdateCommand = new RelayCommand(_ => Update());
            InstallModsCommand = new RelayCommand(_ => InstallMods());
            InstallModCommand = new RelayCommand(_ => InstallMod());
            FilterModsCommand = new RelayCommand(_ => FilterMods());
            RefreshModsCommand = new RelayCommand(_ => RefreshMods());
            ReinstallCommand = new RelayCommand(_ => Reinstall());
            ViewModCommand = new RelayCommand(_ => ViewMod());
            ViewModWebsiteCommand = new RelayCommand(_ => ViewModWebsite());
            CopyModLinkCommand = new RelayCommand(_ => CopyModLink());
            BrowseFolderCommand = new RelayCommand(_ => BrowseFolder());
            DeleteCommand = new RelayCommand(_ => Delete());
        }

        private void UpdateAll()
        {
            Debug.WriteLine("UpdateAll!");
        }

        private void Update()
        {
            Debug.WriteLine("Update!");
        }

        private void InstallMods()
        {
            Debug.WriteLine("InstallMods!");
        }

        private void InstallMod()
        {
            Debug.WriteLine("InstallMod!");
        }

        private void FilterMods()
        {
            Debug.WriteLine("Filter!");

            var filter = ModsFilter.ToLower();

            if (!string.IsNullOrEmpty(filter))
            {
                DisplayedMods = new ObservableCollection<Addon>
                (
                    from addon in modsSource
                    where
                    (
                        addon.Name.ToLower().Contains(filter) ||
                        addon.Category.ToLower().Contains(filter) ||
                        addon.Author.ToLower().Contains(filter)
                    )
                    select addon
                );
            }
            else
            {
                DisplayedMods = new ObservableCollection<Addon>(modsSource);
            }
        }

        private void RefreshMods()
        {
            Debug.WriteLine("Refresh!");
        }

        private void Reinstall()
        {
            Debug.WriteLine("Reinstall!");
        }

        private void ViewMod()
        {
            Debug.WriteLine("ViewMod!");
        }

        private void ViewModWebsite()
        {
            Debug.WriteLine("ViewModWebsite!");
        }

        private void CopyModLink()
        {
            Debug.WriteLine("CopyModLink!");
        }

        private void BrowseFolder()
        {
            Debug.WriteLine("BrowseFolder!");
        }

        private void Delete()
        {
            Debug.WriteLine("Delete!");
        }
    }
}
