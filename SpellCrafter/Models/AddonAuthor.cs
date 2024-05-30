using SQLite;
using SQLiteNetExtensions.Attributes;

namespace SpellCrafter.Models
{
    public class AddonAuthor
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(CommonAddon))]
        public int CommonAddonId { get; set; }

        [ForeignKey(typeof(Author))]
        public int AuthorId { get; set; }
    }
}