using SqlMigrationRunner.Models;
using System;

namespace SqlMigrationRunner.Interfaces
{
    public interface IMigrationManager
    {
        void InitMigrationTable();

        MigrationVersion GetCurrentVersion();

        Migration[] GetAllMigrations();

        void Upgrade(MigrationScript migrationScript);
    }
}
