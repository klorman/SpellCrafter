using SQLite;
using SQLiteNetExtensions.Attributes;

namespace SpellCrafter.Models
{
    public class OnlineAddon
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(CommonAddon))]
        public int CommonAddonId { get; set; }

        [MaxLength(20)]
        public string LatestVersion { get; set; } = string.Empty;

        [MaxLength(20)]
        public string DisplayedLatestVersion { get; set; } = string.Empty;

        [Unique]
        public int? UniqueId { get; set; }

        [OneToOne]
        public CommonAddon CommonAddon { get; set; } = null!;
    }
}
