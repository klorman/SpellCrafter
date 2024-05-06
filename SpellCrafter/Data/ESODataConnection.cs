using LinqToDB;
using LinqToDB.Data;
using SpellCrafter.Models;


namespace SpellCrafter.Data
{
    public class ESODataConnection : DataConnection
    {
        public ESODataConnection() : base("ESO") { }

        public ITable<CommonAddon> CommonAddons => this.GetTable<CommonAddon>();
        public ITable<LocalAddon> LocalAddons => this.GetTable<LocalAddon>();
        public ITable<OnlineAddon> OnlineAddons => this.GetTable<OnlineAddon>();
        public ITable<AddonDependency> AddonDependencies => this.GetTable<AddonDependency>();
        public ITable<Author> Authors => this.GetTable<Author>();
        public ITable<AddonAuthor> AddonAuthors => this.GetTable<AddonAuthor>();
        public ITable<Category> Categories => this.GetTable<Category>();
        public ITable<AddonCategory> AddonCategories => this.GetTable<AddonCategory>();

    }
}
