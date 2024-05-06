using LinqToDB.Mapping;
using SpellCrafter.Enums;

namespace SpellCrafter.Models
{
    [Table(Name = "LocalAddons")]
    public class LocalAddon
    {
        [PrimaryKey, Identity]
        public int Id { get; set; }

        [Column, NotNull]
        public int CommonAddonId { get; set; }

        [Column, NotNull]
        public string Version { get; set; } = string.Empty;

        [Column, NotNull]
        public string DisplayedVersion { get; set; } = string.Empty;

        [Column, NotNull]
        public AddonState State { get; set; } = AddonState.NotInstalled;

        [Column, NotNull]
        public AddonInstallationMethod InstallationMethod { get; set; } = AddonInstallationMethod.Other;

        [Association(ThisKey = nameof(CommonAddonId), OtherKey = nameof(Models.CommonAddon.Id), CanBeNull = false)]
        public CommonAddon CommonAddon { get; set; } = null!;
    }
}
