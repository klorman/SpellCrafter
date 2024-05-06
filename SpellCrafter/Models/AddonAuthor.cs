using LinqToDB.Mapping;

namespace SpellCrafter.Models
{
    [Table(Name = "AddonAuthors")]
    public class AddonAuthor
    {
        [PrimaryKey, Identity]
        public int Id { get; set; }

        [Column, NotNull]
        public int CommonAddonId { get; set; }

        [Column, NotNull]
        public int AuthorId { get; set; }

        [Association(ThisKey = nameof(CommonAddonId), OtherKey = nameof(Models.CommonAddon.Id), CanBeNull = false)]
        public CommonAddon CommonAddon { get; set; } = null!;

        [Association(ThisKey = nameof(AuthorId), OtherKey = nameof(Models.Author.Id), CanBeNull = false)]
        public Author Author { get; set; } = null!;
    }
}