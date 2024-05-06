using LinqToDB.Mapping;

namespace SpellCrafter.Models
{
    [Table(Name = "CommonAddons")]
    public class CommonAddon
    {
        [PrimaryKey, Identity]
        public int Id { get; set; }

        [Column, NotNull]
        public string Name { get; set; } = string.Empty;

        [Column, Nullable]
        public string Description { get; set; } = string.Empty;
    }
}
