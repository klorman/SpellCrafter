using SQLite;
using SQLiteNetExtensions.Attributes;

namespace SpellCrafter.Models
{
    public class AddonCategory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(CommonAddon))]
        public int CommonAddonId { get; set; }
        
        [ForeignKey(typeof(Category))]
        public int CategoryId { get; set; }
    }
}