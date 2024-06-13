using System;
using System.Threading.Tasks;
using ReactiveUI;
using SpellCrafter.Data;
using SpellCrafter.Services;
using Splat;

namespace SpellCrafter.ViewModels
{
    public class BrowseViewModel : AddonsOverviewViewModel, IRoutableViewModel
    {
        public override bool IsScanning => OnlineAddonsParserService.IsScanning;

        public string? UrlPathSegment => "/browse";

        public IScreen HostScreen { get; }

        public BrowseViewModel(IScreen? screen = null) : base(true)
        {
            HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;

            LoadAddons();

            OnlineAddonsParserService.ScanningChanged += OnScanningChanged;
        }

        ~BrowseViewModel()
        {
            OnlineAddonsParserService.ScanningChanged -= OnScanningChanged;
        }

        private void LoadAddons()
        {
            ModsSource = AddonDataManager.OnlineAddons;
        }

        protected override void RescanMods()
        {
            base.RescanMods();

            Task.Run(async () =>
            {
                IsFiltering = true;
                var parser = new OnlineAddonsParserService();
                var addons = await parser.ParseAddonsAsync();
                if (addons != null)
                {
                    using var db = new EsoDataConnection();
                    AddonDataManager.UpdateOnlineAddonsInfo(db, addons);
                }
            });
        }

        private void OnScanningChanged(object? sender, EventArgs e) =>
            this.RaisePropertyChanged(nameof(IsScanning));
    }
}
