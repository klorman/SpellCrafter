namespace SpellCrafter.Models
{
    public interface IOnlineAddon
    {
        public int CommonAddonId { get; set; }

        public string LatestVersion { get; set; }

        public string DisplayedLatestVersion { get; set; }

        public int? UniqueId { get; set; }
    }
}
