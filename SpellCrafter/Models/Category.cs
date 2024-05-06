using LinqToDB.Mapping;

namespace SpellCrafter.Models
{
    [Table(Name = "Categories")]
    public class Category
    {
        [PrimaryKey, Identity]
        public int Id { get; set; }

        [Column, NotNull]
        public string Name { get; set; } = string.Empty;
    }
}