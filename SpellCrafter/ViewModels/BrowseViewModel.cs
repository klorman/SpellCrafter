using System.Threading.Tasks;
using ReactiveUI;
using SpellCrafter.Data;
using SpellCrafter.Services;
using Splat;

namespace SpellCrafter.ViewModels
{
    public class BrowseViewModel : AddonsOverviewViewModel, IRoutableViewModel
    {
        private static bool _isLoading;
        public override bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

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
        }

        protected override void RescanMods()
        {
            var oldIsLoading = IsLoading;
            IsLoading = true;
            base.RescanMods();

            Task.Run(async () =>
            {
                var parser = new OnlineAddonsParserService();
                var addons = await parser.ParseAddonsAsync();
                using var db = new EsoDataConnection();
                AddonDataManager.UpdateOnlineAddonsInfo(db, addons);
                IsLoading = oldIsLoading;
            });
        }
    }
}
