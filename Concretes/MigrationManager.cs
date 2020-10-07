using SqlMigrationRunner.Interfaces;
using SqlMigrationRunner.Models;
using NPoco;
using System;
using System.IO;
using System.Linq;

namespace SqlMigrationRunner.Concretes
{
    public class MigrationManager : IMigrationManager
    {
        private readonly IDatabaseConnectionService _databaseConnectionService;
        private readonly IMigrationScriptConverter _migrationScriptConverter;
        private readonly string _initMigrationScriptFullPath;
        private readonly MigrationStrategy _migrationStrategy;

        public MigrationManager(IDatabaseConnectionService databaseConnectionService, IMigrationScriptConverter migrationScriptConverter, string initMigrationScriptFullPath, MigrationStrategy migrationStrategy) 
        {
            this._databaseConnectionService = databaseConnectionService;
            this._migrationScriptConverter = migrationScriptConverter;
            this._initMigrationScriptFullPath = initMigrationScriptFullPath;
            this._migrationStrategy = migrationStrategy;
        }

        public MigrationVersion GetCurrentVersion()
        {
            string version = string.Empty;

            Sql sql = new Sql("SELECT [Id],");
            sql.Append("		        JSON_VALUE([Version],'$.Major') AS Major,");
            sql.Append("		        JSON_VALUE([Version],'$.Minor') AS Minor,");
            sql.Append("		        JSON_VALUE([Version],'$.Patch') AS Patch");
            sql.Append("      FROM [dbo].[Migration]");
            sql.Append("      ORDER BY Major DESC, Minor DESC, Patch DESC");

            using (IDatabase db = this._databaseConnectionService.GetConnection()) 
            {
                version = db.FirstOrDefault<string>(sql);
            }

            return string.IsNullOrEmpty(version) ? new MigrationVersion() : new MigrationVersion(version);
        }

        public Migration[] GetAllMigrations()
        {
            Models.Db.Migration[] migrations;

            using (IDatabase db = this._databaseConnectionService.GetConnection())
            {
                migrations = db.Fetch<Models.Db.Migration>("SELECT * FROM [dbo].[Migration]").ToArray();
            }

            return migrations.Select(m => new Migration()
            {
                Name = m.Name,
                Timestamp = m.Timestamp,
                Version = new MigrationVersion(m.Id)
            }).ToArray();
        }

        public void InitMigrationTable()
        {
            MigrationScript script = this._migrationScriptConverter.GetMigrationScript(this._initMigrationScriptFullPath);

            if (script == null) 
            {
                throw new FileNotFoundException("Initial Migration Table Script Not Found");
            }

            // make sure table exists
            using (IDatabase db = this._databaseConnectionService.GetConnection())
            {
                db.Execute(new Sql(script.Sql));
            }
        }

        public void Upgrade(MigrationScript migrationScript)
        {
            if (migrationScript == null) 
            {
                throw new ArgumentNullException("migrationScript");
            }

            ValidateMigration(migrationScript);

            using (IDatabase db = this._databaseConnectionService.GetConnection())
            {
                db.BeginTransaction();

                db.Execute(new Sql(migrationScript.Sql));
                db.Execute(GetInsertStatement(migrationScript.Name, migrationScript.Version));

                db.CompleteTransaction();
            }
        }

        private void ValidateMigration(MigrationScript migrationScript)
        {
            switch (this._migrationStrategy)
            {
                case MigrationStrategy.Forward:
                    MigrationVersion currentVersion = this.GetCurrentVersion();

                    if (currentVersion.Version >= migrationScript.Version.Version)
                    {
                        throw new ApplicationException(string.Format("Attempt to target the database to version {0}; the current version {1} is higher",
                                                       migrationScript.Version.ToString(), currentVersion.ToString()));
                    }
                    break;
                case MigrationStrategy.Version:
                    // ignore
                    break;
                default:
                    throw new ApplicationException(string.Format("Migration strategy not recognized {0}", this._migrationStrategy.ToString()));
            }
        }

        private static Sql GetInsertStatement(string name, MigrationVersion version) 
        {
            return new Sql("INSERT INTO [dbo].[Migration] ([Id], [Name], [Version]) VALUES (@0, @1, @2)", version.ToString(), name, version.ToJsonObject());
        }
    }
}
