using LinqToDB.Mapping;

namespace SpellCrafter.Models
{
    [Table(Name = "AddonCategories")]
    public class AddonCategory
    {
        [PrimaryKey, Identity]
        public int Id { get; set; }

        [Column, NotNull]
        public int CommonAddonId { get; set; }
        
        [Column, NotNull]
        public int CategoryId { get; set; }

        [Association(ThisKey = nameof(CommonAddonId), OtherKey = nameof(Models.CommonAddon.Id), CanBeNull = false)]
        public CommonAddon CommonAddon { get; set; } = null!;

        [Association(ThisKey = nameof(CategoryId), OtherKey = nameof(Models.Category.Id), CanBeNull = false)]
        public Category Category { get; set; } = null!;
    }
}