using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace SpellCrafter.Models
{
    public class CommonAddon : ICommonAddon
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [ManyToMany(typeof(AddonDependency))]
        public List<CommonAddon> Dependencies { get; set; } = [];

        [OneToOne]
        public LocalAddon? LocalAddon { get; set; }

        [OneToOne]
        public OnlineAddon? OnlineAddon { get; set; }

        [ManyToMany(typeof(AddonAuthor))]
        public List<Author> Authors { get; set; } = [];
        IList<Author> ICommonAddon.Authors => Authors;

        [ManyToMany(typeof(AddonCategory))]
        public List<Category> Categories { get; set; } = [];
        IList<Category> ICommonAddon.Categories => Categories;
    }
}
