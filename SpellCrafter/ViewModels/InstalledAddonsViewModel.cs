using System;
using ReactiveUI;
using SpellCrafter.Data;
using SpellCrafter.Services;
using Splat;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SpellCrafter.ViewModels
{
    public class InstalledAddonsViewModel : AddonsOverviewViewModel, IRoutableViewModel
    {
        public override bool IsScanning => LocalAddonsScannerService.IsScanning;

        public string? UrlPathSegment => "/installed";

        public IScreen HostScreen { get; }

        public InstalledAddonsViewModel(IScreen? screen = null) : base(false)
        {
            HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;

            LoadLocalAddons();

            LocalAddonsScannerService.ScanningChanged += OnScanningChanged;
        }

        ~InstalledAddonsViewModel()
        {
            LocalAddonsScannerService.ScanningChanged -= OnScanningChanged;
        }

        private void LoadLocalAddons()
        {
            var addonsDirectory = AppSettings.Instance.AddonsDirectory;

            if (string.IsNullOrEmpty(addonsDirectory))
            {
                Debug.WriteLine("AddonsDirectory is empty!");
                return;
            }

            ModsSource = AddonDataManager.InstalledAddons;
        }

        protected override void RescanMods()
        {
            base.RescanMods();

            Task.Run(() =>
            {
                IsFiltering = true;
                var addons = LocalAddonsScannerService.ScanDirectory(AppSettings.Instance.AddonsDirectory);
                if (addons != null)
                {
                    using var db = new EsoDataConnection();
                    AddonDataManager.UpdateInstalledAddonsInfo(db, addons);
                }
            });
        }

        private void OnScanningChanged(object? sender, EventArgs e) =>
            this.RaisePropertyChanged(nameof(IsScanning));
    }
}
