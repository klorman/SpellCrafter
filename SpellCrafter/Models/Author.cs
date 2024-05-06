using LinqToDB.Mapping;

namespace SpellCrafter.Models
{
    [Table(Name = "Authors")]
    public class Author
    {
        [PrimaryKey, Identity]
        public int Id { get; set; }

        [Column, NotNull]
        public string Name { get; set; } = string.Empty;
    }
}