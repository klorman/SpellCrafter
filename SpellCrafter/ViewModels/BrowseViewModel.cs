using ReactiveUI;
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
    }
}
