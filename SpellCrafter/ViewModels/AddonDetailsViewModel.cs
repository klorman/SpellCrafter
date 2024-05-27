using ReactiveUI;
using SpellCrafter.Models;
using Splat;

namespace SpellCrafter.ViewModels
{
    public class AddonDetailsViewModel : Addon, IRoutableViewModel
    {
        public string? UrlPathSegment => "/details";

        public IScreen HostScreen { get; }

        public AddonDetailsViewModel(IScreen? screen = null) : base()
        {
            HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;
        }

        public void CopyFromAddon(Addon addon)
        {
            Dependencies = addon.Dependencies;
            CommonAddonId = addon.CommonAddonId;
            Name = addon.Name;
            Description = addon.Description;
            AddonState = addon.AddonState;
            InstallationMethod = addon.InstallationMethod;
            Downloads = addon.Downloads;
            Categories = addon.Categories;
            Authors = addon.Authors;
            UniqueIdentifier = addon.UniqueIdentifier;
            FileSize = addon.FileSize;
            Overview = addon.Overview;
            Version = addon.Version;
            DisplayedVersion = addon.DisplayedVersion;
            LatestVersion = addon.LatestVersion;
            DisplayedLatestVersion = addon.DisplayedLatestVersion;
            GameVersion = addon.GameVersion;
        }
    }
}
