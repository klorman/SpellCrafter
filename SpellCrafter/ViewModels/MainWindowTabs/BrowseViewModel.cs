using SpellCrafter.Enums;
using System.Diagnostics;

namespace SpellCrafter.ViewModels.MainWindowTabs
{
    public class BrowseViewModel : AddonsOverviewViewModel
    {
        public BrowseViewModel() : base(true)
        {
            LoadAddons();
        }

        private void LoadAddons()
        {
            // Load full addons list

            //modsSource.Add(new() { Name = "CustomCompassPins", ArchiveName = "CustomCompassPins.zip", AddonState = AddonState.NotInstalled, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21", Description = "Slight improvements to the default experience bar that adds current/max experience and always displays the bar Slight improvements to the default experience bar that adds current/max experience and always displays the bar", FileSize = "1K", Downloads = 1625, Overview = "Slight improvements to the default experience bar that adds current/max experience and always displays the bar\n\nSlight improvements to the default experience bar that adds current/max experience and always displays the bar" });
            //modsSource.Add(new() { Name = "LibAddonMenu", ArchiveName = "LibAddonMenu.zip", AddonState = AddonState.LatestVersion, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21", Description = "Slight improvements to the default experience bar that adds current/max experience and always displays the bar", FileSize = "1K", Downloads = 1625 });
            //modsSource.Add(new() { Name = "MiniMap by Fyrakin", ArchiveName = "MiniMap by Fyrakin.zip", AddonState = AddonState.Outdated, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21", Description = "Slight improvements to the default experience bar that adds current/max experience and always displays the bar", FileSize = "1K", Downloads = 1625 });
            //modsSource.Add(new() { Name = "LibLazyCrafting", ArchiveName = "LibLazyCrafting.zip", AddonState = AddonState.Updating, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21", Description = "Slight improvements to the default experience bar that adds current/max experience and always displays the bar", FileSize = "1K", Downloads = 1625 });
            //modsSource.Add(new() { Name = "CustomCompassPins", ArchiveName = "CustomCompassPins.zip", AddonState = AddonState.UpdateError, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21", Description = "Slight improvements to the default experience bar that adds current/max experience and always displays the bar", FileSize = "1K", Downloads = 1625 });
            //modsSource.Add(new() { Name = "LibAddonMenu", ArchiveName = "LibAddonMenu.zip", AddonState = AddonState.LatestVersion, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21", Description = "Slight improvements to the default experience bar that adds current/max experience and always displays the bar", FileSize = "1K", Downloads = 1625 });
            //modsSource.Add(new() { Name = "MiniMap by Fyrakin", ArchiveName = "MiniMap by Fyrakin.zip", AddonState = AddonState.LatestVersion, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21", Description = "Slight improvements to the default experience bar that adds current/max experience and always displays the bar", FileSize = "1K", Downloads = 1625 });
            //modsSource.Add(new() { Name = "LibLazyCrafting", ArchiveName = "LibLazyCrafting.zip", AddonState = AddonState.LatestVersion, Latest = "12/15/2016", Category = "Map", GameVersion = "8.0", Author = "mastropos21", Description = "Slight improvements to the default experience bar that adds current/max experience and always displays the bar", FileSize = "1K", Downloads = 1625 });
            FilterMods();
        }
    }
}
