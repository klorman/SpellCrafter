using LinqToDB.Mapping;

namespace SpellCrafter.Models
{
    [Table(Name = "OnlineAddons")]
    public class OnlineAddon
    {
        [PrimaryKey, Identity]
        public int Id { get; set; }

        [Column, NotNull]
        public int CommonAddonId { get; set; }

        [Column, NotNull]
        public string LatestVersion { get; set; } = string.Empty;

        [Column, NotNull]
        public string DisplayedLatestVersion { get; set; } = string.Empty;

        [Column, NotNull]
        public string UniqueIdentifier { get; set; } = string.Empty;

        [Association(ThisKey = nameof(CommonAddonId), OtherKey = nameof(Models.CommonAddon.Id), CanBeNull = false)]
        public CommonAddon CommonAddon { get; set; } = null!;
    }
}
