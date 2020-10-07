using System;

namespace SqlMigrationRunner.Models.Db
{
    public class Migration
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public DateTime Timestamp { get; set; }

        public string Version { get; set; }
    }
}
