using SQLite;
using SQLiteNetExtensions.Attributes;

namespace SpellCrafter.Models
{
    public class AddonDependency
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(CommonAddon))]
        public int CommonAddonId { get; set; }

        [ForeignKey(typeof(CommonAddon))]
        public int DependentCommonAddonId { get; set; }
    }
}