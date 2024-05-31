using System;
using ReactiveUI.Fody.Helpers;
using SpellCrafter.Data;
using SpellCrafter.Models;
using SpellCrafter.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using ReactiveUI;

namespace SpellCrafter.ViewModels
{
    public class AddonsOverviewViewModel : ViewModelBase
    {
        protected List<Addon> ModsSource = [];
        [Reactive] public ObservableCollection<Addon> DisplayedMods { get; set; } = [];
        [Reactive] public string ModsFilter { get; set; } = string.Empty;
        [Reactive] public bool BrowseMode { get; set; }
        [Reactive] public Addon? DataGridModsSelectedItem { get; set; }
        public bool IsAddonsDisplayed => DisplayedMods.Count > 0;

        public RelayCommand UpdateAllCommand { get; }
        public RelayCommand FilterModsCommand { get; }
        public RelayCommand RefreshModsCommand { get; }

        public AddonsOverviewViewModel(bool browseMode) : base()
        {
            BrowseMode = browseMode;

            UpdateAllCommand = new RelayCommand(_ => UpdateAll());
            FilterModsCommand = new RelayCommand(_ => FilterMods());
            RefreshModsCommand = new RelayCommand(_ => RefreshMods());

            this.WhenAnyValue(x => x.DisplayedMods.Count)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(IsAddonsDisplayed)));
        }

        private void UpdateAll()
        {
            Debug.WriteLine("UpdateAll!");
        }

        protected void FilterMods()
        {
            Debug.WriteLine("Filter!");
            
            if (!string.IsNullOrEmpty(ModsFilter))
            {
                DisplayedMods = new ObservableCollection<Addon>
                (
                    from addon in ModsSource
                    where
                        addon.Name.Contains(ModsFilter, StringComparison.OrdinalIgnoreCase) ||
                        addon.Categories.Any(category => category.Name.Contains(ModsFilter, StringComparison.OrdinalIgnoreCase)) ||
                        addon.Authors.Any(author => author.Name.Contains(ModsFilter, StringComparison.OrdinalIgnoreCase))
                    
                    select addon
                );
            }
            else
            {
                DisplayedMods = new ObservableCollection<Addon>(ModsSource);
            }
        }

        protected virtual void RefreshMods()
        {
            Debug.WriteLine("RefreshMods!");
        }
    }
}
