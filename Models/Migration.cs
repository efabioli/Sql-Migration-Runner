using System;

namespace SqlMigrationRunner.Models
{
    public class Migration
    {
        public string Name { get; set; }

        public DateTime Timestamp { get; set; }

        public MigrationVersion Version { get; set; }
    }
}
