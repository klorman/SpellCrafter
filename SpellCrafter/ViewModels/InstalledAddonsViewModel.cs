using ReactiveUI;
using SpellCrafter.Data;
using SpellCrafter.Services;
using Splat;
using System.Diagnostics;
using System.Reactive.Disposables;

namespace SpellCrafter.ViewModels
{
    public class InstalledAddonsViewModel : AddonsOverviewViewModel, IActivatableViewModel, IRoutableViewModel
    {
        private bool _isInitialized = false;
        public ViewModelActivator Activator { get; } = new();

        public string? UrlPathSegment => "/installed";

        public IScreen HostScreen { get; }

        public InstalledAddonsViewModel(IScreen? screen = null) : base(false)
        {
            HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;

            this.WhenActivated((CompositeDisposable disposable) =>
            {
                if (!_isInitialized)
                {
                    _isInitialized = LoadLocalAddons();
                }
            });

            RefreshModsCommand = new RelayCommand(_ => LoadLocalAddons());
        }

        private bool LoadLocalAddons()
        {
            var addonsDirectory = AppSettings.Instance.AddonsDirectory;

            if (string.IsNullOrEmpty(addonsDirectory))
            {
                Debug.WriteLine("AddonsDirectory is empty!");
                return false;
            }

            //AddonsScannerService.ScanAndSyncLocalAddons(addonsDirectory);

            using var db = new EsoDataConnection();
            ModsSource = AddonDataManager.GetAllLocalAddonsWithDetails(db);
            FilterMods();

            return true;
        }
    }
}
