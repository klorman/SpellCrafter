using LinqToDB;
using ReactiveUI;
using SpellCrafter.Data;
using SpellCrafter.Models;
using SpellCrafter.Services;
using Splat;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;

namespace SpellCrafter.ViewModels
{
    public class InstalledAddonsViewModel : AddonsOverviewViewModel, IActivatableViewModel, IRoutableViewModel
    {
        private bool isInitialized = false;
        public ViewModelActivator Activator { get; } = new();

        public string? UrlPathSegment => "/installed";

        public IScreen HostScreen { get; }

        public InstalledAddonsViewModel(IScreen? screen = null) : base(false)
        {
            HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;

            this.WhenActivated((CompositeDisposable disposable) =>
            {
                if (!isInitialized)
                {
                    isInitialized = LoadLocalAddons();
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

            //AddonsScannerService.ScanAndUpdateDatabase(addonsDirectory);

            using (var db = new ESODataConnection())
            {
                var query = from local in db.LocalAddons
                            join commonAddon in db.CommonAddons on local.CommonAddonId equals commonAddon.Id
                            join online in db.OnlineAddons on commonAddon.Id equals online.CommonAddonId into onlines
                            from online in onlines.DefaultIfEmpty()
                            select new Addon
                            {
                                CommonAddonId = commonAddon.Id,
                                Name = commonAddon.Name,
                                Description = commonAddon.Description,
                                AddonState = local.State,
                                InstallationMethod = local.InstallationMethod,
                                Categories = new(db.AddonCategories.Where(ac => ac.CommonAddonId == commonAddon.Id).Select(ac => ac.Category).ToList()),
                                Authors = new(db.AddonAuthors.Where(aa => aa.CommonAddonId == commonAddon.Id).Select(aa => aa.Author).ToList()),
                                UniqueIdentifier = online.UniqueIdentifier,
                                Version = local.Version,
                                DisplayedVersion = local.DisplayedVersion,
                                LatestVersion = online.LatestVersion,
                                DisplayedLatestVersion = online.DisplayedLatestVersion,
                                Dependencies = db.AddonDependencies.Where(ad => ad.CommonAddonId == commonAddon.Id).Select(ad => ad.DependentAddon).ToList(), // TODO возможно достаточно достать список идентификаторов
                            };
                modsSource = query.ToList();
                FilterMods();
            }

            return true;
        }
    }
}
