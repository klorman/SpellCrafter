using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpellCrafter.Enums;
using SpellCrafter.Messages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace SpellCrafter.ViewModels.MainWindowTabs
{
    public class AddonsOverviewViewModel : ViewModelBase
    {
        protected List<Addon> modsSource = new();
        [Reactive] public ObservableCollection<Addon> DisplayedMods { get; set; } = new();
        [Reactive] public string ModsFilter { get; set; } = "";
        [Reactive] public bool BrowseMode { get; set; }
        [Reactive] public Addon? DataGridModsSelectedItem { get; set; }

        public ICommand UpdateAllCommand { get; }
        public ICommand FilterModsCommand { get; }
        public ICommand RefreshModsCommand { get; }
        public ICommand ViewModCommand { get; }

        public AddonsOverviewViewModel(bool browseMode) : base()
        {
            BrowseMode = browseMode;

            UpdateAllCommand = new RelayCommand(_ => UpdateAll());
            FilterModsCommand = new RelayCommand(_ => FilterMods());
            RefreshModsCommand = new RelayCommand(_ => RefreshMods());
            ViewModCommand = new RelayCommand(_ => ViewMod());
        }

        private void UpdateAll()
        {
            Debug.WriteLine("UpdateAll!");
        }

        protected void FilterMods()
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
            Debug.WriteLine("RefreshMods!");
        }

        private void ViewMod()
        {
            Debug.WriteLine("ViewMod!");

            MessageBus.Current.SendMessage(new AddonUpdatedMessage(DataGridModsSelectedItem));
        }
    }
}
