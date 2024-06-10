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
        private static bool _isLoading;
        public override bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

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
        }

        protected override void RescanMods()
        {
            var oldIsLoading = IsLoading;
            IsLoading = true;
            base.RescanMods();

            Task.Run(() =>
            {
                var addons = LocalAddonsScannerService.ScanDirectory(AppSettings.Instance.AddonsDirectory);
                using var db = new EsoDataConnection();
                AddonDataManager.UpdateInstalledAddonsInfo(db, addons);
                IsLoading = oldIsLoading;
            });
        }
    }
}
