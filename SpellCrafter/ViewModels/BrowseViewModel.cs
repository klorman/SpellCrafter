using System.Threading.Tasks;
using ReactiveUI;
using SpellCrafter.Data;
using SpellCrafter.Services;
using Splat;

namespace SpellCrafter.ViewModels
{
    public class BrowseViewModel : AddonsOverviewViewModel, IRoutableViewModel
    {
        public string? UrlPathSegment => "/browse";

        public IScreen HostScreen { get; }

        public BrowseViewModel(IScreen? screen = null) : base(true)
        {
            HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;

            LoadAddons();
        }

        private void LoadAddons()
        {
            ModsSource = AddonDataManager.OnlineAddons;
            IsLoading = false;
        }

        protected override void RescanMods()
        {
            IsLoading = true;
            base.RescanMods();

            Task.Run(async () =>
            {
                var parser = new OnlineAddonsParserService();
                var addons = await parser.ParseAddonsAsync();
                using var db = new EsoDataConnection();
                AddonDataManager.UpdateOnlineAddonsInfo(db, addons);
                IsLoading = false;
            });
        }
    }
}
