using LinqToDB.Mapping;

namespace SpellCrafter.Models
{
    [Table(Name = "AddonDependencies")]
    public class AddonDependency
    {
        [PrimaryKey, Identity]
        public int Id { get; set; }

        [Column, NotNull]
        public int CommonAddonId { get; set; }

        [Column, NotNull]
        public int DependentAddonId { get; set; }

        [Association(ThisKey = nameof(CommonAddonId), OtherKey = nameof(Models.CommonAddon.Id), CanBeNull = false)]
        public CommonAddon CommonAddon { get; set; } = null!;

        [Association(ThisKey = nameof(DependentAddonId), OtherKey = nameof(Models.CommonAddon.Id), CanBeNull = false)]
        public CommonAddon DependentAddon { get; set; } = null!;
    }
}