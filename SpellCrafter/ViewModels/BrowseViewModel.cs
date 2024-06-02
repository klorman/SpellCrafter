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
            FilterMods();
        }

        protected override async void RefreshMods()
        {
            base.RefreshMods();

            var parser = new OnlineAddonsParserService();
            var addons = await parser.ParseAddonsAsync();
            AddonDataManager.OnlineAddons.Clear();
            AddonDataManager.OnlineAddons.AddRange(addons);
            FilterMods();
        }
    }
}
