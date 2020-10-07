using SqlMigrationRunner.Concretes;
using SqlMigrationRunner.Interfaces;
using SqlMigrationRunner.Models;
using NPoco;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;

namespace SqlMigrationRunner
{
    class Program
    {
        private static Settings _settings;
        private static IDatabaseConnectionService _databaseConnection;
        private static IMigrationScriptConverter _migrationScriptConverter;
        private static IMigrationManager _migrationManager;
        private static string _migrationScriptsFolder;
        private static string _artifactContentFolder;

        static void Main(string[] args)
        {
            try
            {
                Init(args);
                Run();
            }
            catch (Exception ex)
            {

                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                Exit(false, "==================", _artifactContentFolder);

                if (_settings != null && !_settings.DebugMode) 
                {
                    throw ex;
                }
            }
        }

        private static void Init(string[] args)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string initMigrationScriptPath = string.Format("{0}{1}", baseDirectory, "V0.0.0_InitMigrationTable.sql");
            _artifactContentFolder = string.Format("{0}{1}", baseDirectory, "artifact");

            Console.WriteLine("========== Row Settings ==========");
            Console.WriteLine(args == null ? string.Empty : string.Join(" - ", args));
            Console.WriteLine("========================================");

            _settings = new Settings(args);

            if (_settings == null)
            {
                throw new ArgumentException("Args empty or not valid");
            }

            Console.WriteLine("========== Settings ==========");
            Console.WriteLine(JsonSerializer.Serialize(_settings, new JsonSerializerOptions() { WriteIndented = true, IgnoreNullValues = true }));
            Console.WriteLine("========================================");

            WaitForKey(_settings.DebugMode);

            ExtractArtifactTo(_settings.ArtifactPath, _artifactContentFolder);
            _migrationScriptsFolder = GetScriptMigrationFolderPath(_artifactContentFolder, _settings.MigrationScriptsFolderName);
            
            _databaseConnection = new DatabaseConnectionService(_settings.ConnectionString, DatabaseType.SqlServer2012, SqlClientFactory.Instance);
            _migrationScriptConverter = new MigrationScriptConverter();
            _migrationManager = new MigrationManager(_databaseConnection, _migrationScriptConverter, initMigrationScriptPath, _settings.MigrationStrategy);
        }

        private static void Run()
        {
            // create migration table (if it does not exist)
            _migrationManager.InitMigrationTable();

            // retrieve current version
            MigrationVersion currentVersion = _migrationManager.GetCurrentVersion();

            // retrieve sql migration scripts to run
            MigrationScript[] _artifactScripts = _migrationScriptConverter.GetMigrationScripts(_migrationScriptsFolder, _settings.IncludeSubFolders);

            if (_artifactScripts == null || !_artifactScripts.Any())
            {
                Exit(_settings.DebugMode, "No sql migration scripts found", _artifactContentFolder);
                return;
            }

            MigrationScript[] scriptsToRun = GetScriptsToRun(_artifactScripts, currentVersion);

            if (!scriptsToRun.Any())
            {
                Exit(_settings.DebugMode, "Database is up to date, no upgraded required", _artifactContentFolder);
                return;
            }

            // run scripts
            foreach (MigrationScript script in scriptsToRun)
            {
                Console.WriteLine(string.Format("Applying migration {0} version", script.Version.ToString()));
                _migrationManager.Upgrade(script);
                Console.WriteLine(string.Format("Migration {0} version done", script.Version.ToString()));
                Console.WriteLine();
            }

            currentVersion = _migrationManager.GetCurrentVersion();

            Exit(_settings.DebugMode, string.Format("Database version = '{0}'", currentVersion.ToString()), _artifactContentFolder);
            return;
        }

        static MigrationScript[] GetScriptsToRun(MigrationScript[] scripts, MigrationVersion currentVersion)
        {
            IEnumerable<MigrationScript> scriptsToRun = null;

            switch (_settings.MigrationStrategy)
            {
                case MigrationStrategy.Version:
                    string[] scriptsRun = _migrationManager.GetAllMigrations().Select(m => m.Version.ToString().ToLower()).ToArray();

                    scriptsToRun = scripts.Where(x => (_settings.VersionThreshold == null || x.Version.Version <= _settings.VersionThreshold.Version) && 
                                                      !scriptsRun.Contains(x.Version.ToString().ToLower()));
                    break;
                default:
                    scriptsToRun = scripts.Where(x => x.Version.Version > currentVersion.Version && (_settings.VersionThreshold == null || x.Version.Version <= _settings.VersionThreshold.Version));
                    break;
            }

            return scriptsToRun.OrderBy(x => x.Version.Version).ToArray();
        }

        static string GetScriptMigrationFolderPath(string artifactContentFolder, string migrationFolderName)
        {
            string[] directories = Directory.GetDirectories(artifactContentFolder, migrationFolderName, SearchOption.AllDirectories);

            if (directories == null || directories.Length == 0)
            {
                throw new DirectoryNotFoundException(string.Format("Script Migration not found: {0} - pattern: {1}", artifactContentFolder, migrationFolderName));
            }

            if (directories.Length > 2)
            {
                throw new DirectoryNotFoundException(string.Format("Multiple folders called {0}.", migrationFolderName));
            }

            return directories.First();
        }

        static void ExtractArtifactTo(string artifactPath, string extractTo)
        {
            if (!File.Exists(artifactPath))
            {
                throw new FileNotFoundException(string.Format("Artifact not found: '{0}'", artifactPath));
            }

            ZipFile.ExtractToDirectory(artifactPath, extractTo);
        }

        static void Exit(bool debugMode, string message, string artifactContentFolder)
        {
            Console.WriteLine(message);
            Console.WriteLine();
            Directory.Delete(artifactContentFolder, true);
            WaitForKey(debugMode);
        }

        static void WaitForKey(bool debugMode)
        {
            if (debugMode)
            {
                Console.WriteLine("Press any key to continue");
                Console.WriteLine();
                Console.ReadKey();
            }
        }
    }
}
