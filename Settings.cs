using SqlMigrationRunner.Models;
using System;
using System.Linq;

namespace SqlMigrationRunner
{
    public class Settings
    {
        public Settings(string[] args)
        {
            if (args == null)
            {
                return;
            }

            this.Init(args);
        }

        public string ConnectionString { get; private set; }

        public string MigrationScriptsFolderName { get; private set; }

        public string ArtifactPath { get; private set; }

        public bool IncludeSubFolders { get; private set; }

        public MigrationVersion VersionThreshold { get; private set; }

        public MigrationStrategy MigrationStrategy { get; set; }

        public bool DebugMode { get; private set; }

        private void Init(string[] args) 
        {
            foreach (string arg in args.Select(a => (a ?? string.Empty).Trim()))
            {
                if (string.IsNullOrEmpty(arg))
                {
                    continue;
                }

                int tokenIndex = arg.IndexOf('=');

                if (tokenIndex < 0)
                {
                    throw new ArgumentException(string.Format("Argument '{0}' not valid", arg));
                }

                this.Set(arg.Substring(0, tokenIndex), arg.Substring(tokenIndex + 1));
            }
        }

        private void Set(string key, string value) 
        {
            switch (key.ToLower().Replace("/", string.Empty))
            {
                case "cs":
                    this.ConnectionString = value;
                    break;
                case "artifactpath":
                    this.ArtifactPath = value;
                    break;
                case "foldername":
                    this.MigrationScriptsFolderName= value;
                    break;
                case "includesubfolder":
                    this.IncludeSubFolders = bool.Parse(value);
                    break;
                case "migrationstrategy":
                    MigrationStrategy sTemp;

                    if (Enum.TryParse(value, true, out sTemp)) 
                    {
                        this.MigrationStrategy = sTemp;
                    }
                    break;
                case "versionthreshold":

                    if (!string.IsNullOrEmpty(value)) 
                    {
                        this.VersionThreshold = new MigrationVersion(value);
                    }
                    break;
                case "debug":
                    this.DebugMode = bool.Parse(value);
                    break;
                default:
                    break;
            }
        }
    }
}
