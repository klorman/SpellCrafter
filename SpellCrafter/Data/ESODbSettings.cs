using LinqToDB;
using LinqToDB.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace SpellCrafter.Data
{
    public class ESODbSettings : ILinqToDBSettings
    {
        public IEnumerable<IDataProviderSettings> DataProviders => 
            Enumerable.Empty<IDataProviderSettings>();

        public string DefaultConfiguration => ProviderName.SQLite;

        public string DefaultDataProvider => ProviderName.SQLite;

        public IEnumerable<IConnectionStringSettings> ConnectionStrings
        {
            get
            {
                yield return
                    new ConnectionStringSettings
                    {
                        Name = "ESO",
                        ProviderName = ProviderName.SQLite,
                        ConnectionString = "Data Source=ESOAddons.db"
                    };
            }
        }
    }
}
