using SpellCrafter.Enums;

namespace SpellCrafter.Models
{
    public interface ILocalAddon
    {
        public int CommonAddonId { get; set; }

        public string Version { get; set; }

        public string DisplayedVersion { get; set; }

        public AddonState State { get; set; }

        public AddonInstallationMethod InstallationMethod { get; set; }
    }
}
