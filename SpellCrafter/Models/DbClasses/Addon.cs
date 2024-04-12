using ReactiveUI.Fody.Helpers;
using SpellCrafter.Enums;

namespace SpellCrafter.Models.DbClasses
{
    public class Addon
    {
        [Reactive] public long Id { get; set; }
        [Reactive] public string Name { get; set; } = "";
        [Reactive] public string ArchiveName { get; set; } = "";
        [Reactive] public AddonState AddonState { get; set; } = AddonState.NotInstalled;
        [Reactive] public int Downloads { get; set; } = 0;
        [Reactive] public string Category { get; set; } = "";
        [Reactive] public string Latest { get; set; } = "";
        [Reactive] public string GameVersion { get; set; } = "";
        [Reactive] public string Author { get; set; } = "";
        [Reactive] public string Link { get; set; } = "";
        [Reactive] public string Description { get; set; } = "";
        [Reactive] public string FileSize { get; set; } = "";
        [Reactive] public string Overview { get; set; } = "";
    }
}
