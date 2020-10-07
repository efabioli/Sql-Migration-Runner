using System;
using System.Linq;
using System.Text.Json;

namespace SqlMigrationRunner.Models
{
    public sealed class MigrationVersion
    {
        public int Major { get; set; }

        public int Minor { get; set; }

        public int Patch { get; set; }

        public MigrationVersion() { }
        
        public MigrationVersion(string version)
            : this()
        {
            int[] tokens = GetVersionTokens(version);
            this.Major = tokens[0];
            this.Minor = tokens[1];
            this.Patch = tokens[2];
        }

        public MigrationVersion(int major, int minor, int patch)
            : this()
        {
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
        }

        public Version Version 
        {
            get 
            {
                return new Version(this.Major, this.Minor, this.Patch);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}", this.Major, this.Minor, this.Patch);
        }

        public string ToJsonObject()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true });
        }

        private static int[] GetVersionTokens(string version) 
        {
            int temp;
            int[] versionTokens = (version ?? string.Empty).ToLower()
            .Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(x => int.TryParse(x, out temp))
            .Select(int.Parse)
            .ToArray();

            if (versionTokens.Length != 3)
            {
                throw new ArgumentException(string.Format("versionThreshold parameter '{0}' not valid", version ?? string.Empty));
            }

            return versionTokens;
        }
    }
}
