using ReactiveUI.Fody.Helpers;
using SpellCrafter.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace SpellCrafter.ViewModels
{
    public class AddonsOverviewViewModel : ViewModelBase
    {
        protected List<Addon> modsSource = new();
        [Reactive] public ObservableCollection<Addon> DisplayedMods { get; set; } = new();
        [Reactive] public string ModsFilter { get; set; } = "";
        [Reactive] public bool BrowseMode { get; set; }
        [Reactive] public Addon? DataGridModsSelectedItem { get; set; }

        public RelayCommand UpdateAllCommand { get; }
        public RelayCommand FilterModsCommand { get; }
        public RelayCommand RefreshModsCommand { get; protected set; }

        public AddonsOverviewViewModel(bool browseMode) : base()
        {
            BrowseMode = browseMode;

            UpdateAllCommand = new RelayCommand(_ => UpdateAll());
            FilterModsCommand = new RelayCommand(_ => FilterMods());
            RefreshModsCommand = new RelayCommand(_ => RefreshMods());
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
                    
                        addon.Name.ToLower().Contains(filter) ||
                        addon.Categories.Any(category => category.Name.ToLower().Contains(filter)) ||
                        addon.Authors.Any(author => author.Name.ToLower().Contains(filter))
                    
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
    }
}
