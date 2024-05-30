using SpellCrafter.Models;
using SQLite;
using System;

namespace SpellCrafter.Data
{
    public class EsoDataConnection() : SQLiteConnection("ESOAddons.db")
    {
        /// <summary>
        /// Ensures that a database table is created only if it does not already exist.
        /// This function is particularly necessary when publishing with native AOT,
        /// where the CreateTable method attempts to create the table regardless of its existence..
        /// </summary>
        public bool CheckTableExists<T>()
        {
            try
            {
                ExecuteScalar<string>($"SELECT name FROM sqlite_master WHERE type='table' AND name='{typeof(T).Name}'");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void CreateTableIfNotExists<T>()
        {
            if (CheckTableExists<T>())
                CreateTable<T>();
        }

        public static void CreateTablesIfNotExists()
        {
            using var db = new EsoDataConnection();
            db.CreateTableIfNotExists<CommonAddon>();
            db.CreateTableIfNotExists<LocalAddon>();
            db.CreateTableIfNotExists<OnlineAddon>();
            db.CreateTableIfNotExists<Author>();
            db.CreateTableIfNotExists<Category>();
            db.CreateTableIfNotExists<AddonAuthor>();
            db.CreateTableIfNotExists<AddonCategory>();
            db.CreateTableIfNotExists<AddonDependency>();
        }
    }
}
