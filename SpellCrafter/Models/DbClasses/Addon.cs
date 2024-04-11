using ReactiveUI.Fody.Helpers;
using SpellCrafter.Enums;

namespace SpellCrafter.Models.DbClasses
{
    public class Addon
    {
        [Reactive] public long Id { get; set; }
        [Reactive] public string Name { get; set; } = "";
        [Reactive] public string ArchiveName { get; set; } = "";
        [Reactive] public AddonActions AddonAction { get; set; } = AddonActions.Unknown;
        [Reactive] public int Downloads { get; set; } = 0;
        [Reactive] public string Category { get; set; } = "";
        [Reactive] public string Latest { get; set; } = "";
        [Reactive] public string GameVersion { get; set; } = "";
        [Reactive] public string Author { get; set; } = "";
        [Reactive] public string Link { get; set; } = "";
    }
}
