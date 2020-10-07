using System;

namespace SqlMigrationRunner.Models
{
    public class MigrationScript
    {
        public string Name { get; set; }

        public string Sql { get; set; }

        public MigrationVersion Version { get; set; }
    }
}
