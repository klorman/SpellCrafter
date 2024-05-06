using LinqToDB.Configuration;

namespace SpellCrafter.Data
{
    public class ConnectionStringSettings : IConnectionStringSettings
    {
        public string ConnectionString { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? ProviderName { get; set; }

        public bool IsGlobal => false;
    }
}
