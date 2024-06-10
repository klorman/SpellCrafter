using ReactiveUI;
using SpellCrafter.Models;
using Splat;

namespace SpellCrafter.ViewModels
{
    public class AddonDetailsViewModel(IScreen? screen = null) : Addon, IRoutableViewModel
    {
        public string? UrlPathSegment => "/details";

        public IScreen HostScreen { get; } = screen ?? Locator.Current.GetService<IScreen>()!;

        public void CopyFromAddon(Addon addon)
        {
            LocalDependencies = addon.LocalDependencies;
            OnlineDependencies = addon.OnlineDependencies;
            CommonAddonId = addon.CommonAddonId;
            Name = addon.Name;
            Title = addon.Title;
            Description = addon.Description;
            State = addon.State;
            InstallationMethod = addon.InstallationMethod;
            Downloads = addon.Downloads;
            Categories = addon.Categories;
            Authors = addon.Authors;
            UniqueId = addon.UniqueId;
            FileSize = addon.FileSize;
            Overview = addon.Overview;
            Version = addon.Version;
            DisplayedVersion = addon.DisplayedVersion;
            LatestVersion = addon.LatestVersion;
            DisplayedLatestVersion = addon.DisplayedLatestVersion;
        }
    }
}
