using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace SpellCrafter.Models
{
    public class Category
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(50), Unique]
        public string Name { get; set; } = string.Empty;

        [ManyToMany(typeof(AddonCategory))]
        public List<CommonAddon> CommonAddons { get; set; } = [];
    }
}