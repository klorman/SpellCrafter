using LinqToDB;
using SpellCrafter.Models;

namespace SpellCrafter.Data
{
    public class ESODbInitializer
    {
        public static void CreateTablesIfNotExists()
        {
            using (var db = new ESODataConnection())
            {
                db.CreateTable<CommonAddon>(tableOptions: TableOptions.CheckExistence);
                db.CreateTable<LocalAddon>(tableOptions: TableOptions.CheckExistence);
                db.CreateTable<OnlineAddon>(tableOptions: TableOptions.CheckExistence);
                db.CreateTable<AddonDependency>(tableOptions: TableOptions.CheckExistence);
                db.CreateTable<Author>(tableOptions: TableOptions.CheckExistence);
                db.CreateTable<AddonAuthor>(tableOptions: TableOptions.CheckExistence);
                db.CreateTable<Category>(tableOptions: TableOptions.CheckExistence);
                db.CreateTable<AddonCategory>(tableOptions: TableOptions.CheckExistence);
            }
        }
    }
}
