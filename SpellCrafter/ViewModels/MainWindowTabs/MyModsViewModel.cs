using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpellCrafter.Enums;
using SpellCrafter.Models.DbClasses;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Windows.Input;

namespace SpellCrafter.ViewModels.MainWindowTabs
{
    public class MyModsViewModel : ViewModelBase
    {
        private readonly ObservableCollection<Addon> allMods = new();
        [Reactive] public ObservableCollection<Addon> DisplayedMods { get; set; } = new();
        [Reactive] public string ModsFilter { get; set; } = "";
        public ObservableCollection<Addon> DataGridModsSelectedItems { get; set; } = new();

        public ICommand UpdateAllCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand FilterModsCommand { get; }
        public ICommand RefreshModsCommand { get; }
        public ICommand ReinstallCommand { get; }
        public ICommand ViewModCommand { get; }
        public ICommand ViewModWebsiteCommand { get; }
        public ICommand CopyModLinkCommand { get; }
        public ICommand BrowseFolderCommand { get; }

        public MyModsViewModel() : base()
        {
            allMods.Add(new() { Name = "CustomCompassPins", ArchiveName = "CustomCompassPins.zip", AddonAction = AddonActions.UpToDate, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21" });
            allMods.Add(new() { Name = "LibAddonMenu", ArchiveName = "LibAddonMenu.zip", AddonAction = AddonActions.UpToDate, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21" });
            allMods.Add(new() { Name = "MiniMap by Fyrakin", ArchiveName = "MiniMap by Fyrakin.zip", AddonAction = AddonActions.UpToDate, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21" });
            allMods.Add(new() { Name = "LibLazyCrafting", ArchiveName = "LibLazyCrafting.zip", AddonAction = AddonActions.UpToDate, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21" });
            allMods.Add(new() { Name = "CustomCompassPins", ArchiveName = "CustomCompassPins.zip", AddonAction = AddonActions.UpToDate, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21" });
            allMods.Add(new() { Name = "LibAddonMenu", ArchiveName = "LibAddonMenu.zip", AddonAction = AddonActions.UpToDate, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21" });
            allMods.Add(new() { Name = "MiniMap by Fyrakin", ArchiveName = "MiniMap by Fyrakin.zip", AddonAction = AddonActions.UpToDate, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21" });
            allMods.Add(new() { Name = "LibLazyCrafting", ArchiveName = "LibLazyCrafting.zip", AddonAction = AddonActions.UpToDate, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21" });

            FilterMods();

            UpdateAllCommand = new RelayCommand(_ => UpdateAll());
            UpdateCommand = new RelayCommand(_ => Update());
            DeleteCommand = new RelayCommand(_ => Delete());
            FilterModsCommand = new RelayCommand(_ => FilterMods());
            RefreshModsCommand = new RelayCommand(_ => RefreshMods());
            ReinstallCommand = new RelayCommand(_ => Reinstall());
            ViewModCommand = new RelayCommand(_ => ViewMod());
            ViewModWebsiteCommand = new RelayCommand(_ => ViewModWebsite());
            CopyModLinkCommand = new RelayCommand(_ => CopyModLink());
            BrowseFolderCommand = new RelayCommand(_ => BrowseFolder());
        }

        private void UpdateAll()
        {
            Debug.WriteLine("UpdateAll!");
        }

        private void Update()
        {
            Debug.WriteLine("Update!");
        }

        private void Delete()
        {
            Debug.WriteLine("Delete!");
        }

        private void FilterMods()
        {
            Debug.WriteLine("Filter!");

            var filter = ModsFilter.ToLower();

            if (!string.IsNullOrEmpty(filter))
            {
                DisplayedMods = new ObservableCollection<Addon>
                (
                    from addon in allMods
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
                DisplayedMods = new ObservableCollection<Addon>(allMods);
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
    }
}
