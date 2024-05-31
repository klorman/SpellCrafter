using ReactiveUI;
using SpellCrafter.Data;
using SpellCrafter.Services;
using Splat;
using System.Diagnostics;

namespace SpellCrafter.ViewModels
{
    public class InstalledAddonsViewModel : AddonsOverviewViewModel, IRoutableViewModel
    {
        public string? UrlPathSegment => "/installed";

        public IScreen HostScreen { get; }

        public InstalledAddonsViewModel(IScreen? screen = null) : base(false)
        {
            HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;

            LoadLocalAddons();
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
            FilterMods();
        }

        protected override void RefreshMods()
        {
            base.RefreshMods();

            var addons = AddonsScannerService.ScanDirectory(AppSettings.Instance.AddonsDirectory);
            using var db = new EsoDataConnection();
            AddonDataManager.UpdateLocalAddonList(db, addons);
            LoadLocalAddons();
        }
    }
}
