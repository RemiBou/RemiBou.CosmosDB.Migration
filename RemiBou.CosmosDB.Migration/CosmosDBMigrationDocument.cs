using System;

namespace RemiBou.CosmosDB.Migration
{
    public class CosmosDBMigrationDocument
    {
        public CosmosDBMigrationDocument()
        {
        }

        public CosmosDBMigrationDocument(string id)
        {
            Id = id;
            AppliedOn = DateTimeOffset.Now;
        }

        public string Id { get; set; }

        public DateTimeOffset AppliedOn { get; set; }

        public TimeSpan Duration { get; set; }

        public string Checksum { get; set; }
    }
}
