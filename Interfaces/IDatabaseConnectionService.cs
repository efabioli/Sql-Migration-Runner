using NPoco;

namespace SqlMigrationRunner.Interfaces
{
    public interface IDatabaseConnectionService
    {
        IDatabase GetConnection();
    }
}
