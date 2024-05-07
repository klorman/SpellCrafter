using LinqToDB;
using ReactiveUI;
using SpellCrafter.Data;
using SpellCrafter.Enums;
using SpellCrafter.Models;
using SpellCrafter.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Input;

namespace SpellCrafter.ViewModels.MainWindowTabs
{
    public class MyModsViewModel : AddonsOverviewViewModel, IActivatableViewModel
    {
        private bool isInitialized = false;
        public ViewModelActivator Activator { get; } = new();

        public MyModsViewModel() : base(false)
        {
            this.WhenActivated((CompositeDisposable disposable) =>
            {
                if (!isInitialized)
                {
                    LoadLocalAddons();
                    isInitialized = true;
                }
            });

            RefreshModsCommand = new RelayCommand(_ => LoadLocalAddons());
        }

        private async void LoadLocalAddons()
        {
            var addonsDirectory = AppSettings.Instance.AddonsDirectory;

            if (string.IsNullOrEmpty(addonsDirectory))
            {
                Debug.WriteLine("AddonsDirectory is empty!");
                await ShowMainDialogAsync(new AddonFolderSelectionDialogViewModel());
                addonsDirectory = AppSettings.Instance.AddonsDirectory;
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
        }
    }
}
