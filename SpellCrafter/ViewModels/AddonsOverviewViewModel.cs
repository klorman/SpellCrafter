using System;
using ReactiveUI.Fody.Helpers;
using SpellCrafter.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using ReactiveUI;
using SpellCrafter.Enums;

namespace SpellCrafter.ViewModels
{
    public class AddonsOverviewViewModel : ViewModelBase
    {
        private RangedObservableCollection<Addon> _modsSource = [];
        protected RangedObservableCollection<Addon> ModsSource
        {
            get => _modsSource;
            set
            {
                _modsSource = value;
                this.WhenAnyValue(x => x._modsSource.Count)
                    .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
                    .Subscribe(_ => FilterMods());
                FilterMods();
            }
        }

        [Reactive] public virtual bool IsLoading { get; set; }

        [Reactive] public RangedObservableCollection<Addon> DisplayedMods { get; set; } = [];
        [Reactive] public string ModsFilter { get; set; } = string.Empty;
        [Reactive] public bool BrowseMode { get; set; }
        [Reactive] public Addon? DataGridModsSelectedItem { get; set; }
        public bool IsAddonsDisplayed => DisplayedMods.Count > 0;
       
        public RelayCommand UpdateAllCommand { get; }
        public RelayCommand FilterModsCommand { get; }
        public RelayCommand RefreshModsCommand { get; }

        public AddonsOverviewViewModel(bool browseMode)
        {
            BrowseMode = browseMode;

            UpdateAllCommand = new RelayCommand
            (
                _ => UpdateAll(),
                _ => !IsLoading
            );
            FilterModsCommand = new RelayCommand
            (
                _ => FilterMods(),
                _ => !IsLoading
            );
            RefreshModsCommand = new RelayCommand
            (
                _ => RescanMods(),
                _ => !IsLoading
            );
            
            this.WhenAnyValue(x => x.DisplayedMods.Count)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(IsAddonsDisplayed)));
        }

        private async void UpdateAll()
        {
            Debug.WriteLine("Updating all outdated addons");

            var oldIsLoading = IsLoading;
            IsLoading = true;
            foreach (var addon in ModsSource)
            {
                if (addon.State is AddonState.Outdated or AddonState.InstallationError)
                    await addon.Update(false);
            }

            IsLoading = oldIsLoading;
        }

        protected void FilterMods()
        {
            Debug.WriteLine("Filtering displayed addons");

            var oldIsLoading = IsLoading;
            IsLoading = true;

            var filter = ModsFilter.Replace(" ", "");
            List<Addon> filteredAddons;
            if (!string.IsNullOrEmpty(filter))
            {
                filteredAddons = ModsSource.Where(addon =>
                        addon.Name.Replace(" ", "").Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                        addon.Categories.Any(category => category.Name.Replace(" ", "").Contains(filter, StringComparison.OrdinalIgnoreCase)) || // TODO move categories and authors to filters
                        addon.Authors.Any(author => author.Name.Replace(" ", "").Contains(filter, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            else
            {
                filteredAddons = [..ModsSource];
            }
            
            DisplayedMods.Refresh(filteredAddons, false);

            IsLoading = oldIsLoading;
        }

        protected virtual void RescanMods()
        {
            Debug.WriteLine("Rescanning addons");
        }
    }
}
