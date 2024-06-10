using SQLite;
using SQLiteNetExtensions.Attributes;

namespace SpellCrafter.Models
{
    public class OnlineAddonDependency
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(OnlineAddon))]
        public int OnlineAddonId { get; set; }

        [ForeignKey(typeof(CommonAddon))]
        public int DependentCommonAddonId { get; set; }
    }
}
