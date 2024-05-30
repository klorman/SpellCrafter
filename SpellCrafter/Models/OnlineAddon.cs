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

        [MaxLength(100)]
        public string UniqueIdentifier { get; set; } = string.Empty;

        [OneToOne]
        public CommonAddon CommonAddon { get; set; } = null!;
    }
}
