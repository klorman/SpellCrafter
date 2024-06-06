using System.Collections.Generic;

namespace SpellCrafter.Models
{
    public interface ICommonAddon
    {
        public string Name { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        IList<Author> Authors { get; }

        IList<Category> Categories { get; }
    }
}
