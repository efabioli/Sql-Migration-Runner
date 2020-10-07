using SqlMigrationRunner.Interfaces;
using NPoco;
using System.Data.Common;

namespace SqlMigrationRunner.Concretes
{
    public class DatabaseConnectionService : IDatabaseConnectionService
    {
        private readonly string _connectionString;
        private readonly DatabaseType _databaseType;
        private readonly DbProviderFactory _providerFactory;

        public DatabaseConnectionService(string connectionString, DatabaseType databaseType, DbProviderFactory provider)
        {
            this._connectionString = connectionString;
            this._databaseType = databaseType;
            _providerFactory = provider;
        }

        public IDatabase GetConnection()
        {
            return new Database(this._connectionString, this._databaseType, this._providerFactory);
        }
    }
}
