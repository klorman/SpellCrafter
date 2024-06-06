using SpellCrafter.Enums;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace SpellCrafter.Models
{
    public class LocalAddon : ILocalAddon
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int? Id { get; set; }

        [ForeignKey(typeof(CommonAddon))]
        public int CommonAddonId { get; set; }

        [MaxLength(20)]
        public string Version { get; set; } = string.Empty;

        [MaxLength(20)]
        public string DisplayedVersion { get; set; } = string.Empty;

        public AddonState State { get; set; } = AddonState.NotInstalled;

        public AddonInstallationMethod InstallationMethod { get; set; } = AddonInstallationMethod.Other;

        [OneToOne]
        public CommonAddon CommonAddon { get; set; } = null!;
    }
}
