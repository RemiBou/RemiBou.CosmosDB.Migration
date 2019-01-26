using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Threading.Tasks;

namespace RemiBou.CosmosDB.Migration
{
    public class StoredProcedureMigrationStrategy : IMigrationStrategy
    {
        public async Task ApplyMigrationAsync(IDocumentClient client, ParsedMigrationName migration, string content)
        {
            await client.UpsertStoredProcedureAsync(
                UriFactory.CreateDocumentCollectionUri(migration.DataBase.Id, migration.Collection.Id),
                new StoredProcedure()
                {
                    Body = content,
                    Id = migration.Name
                });
        }

        public bool Handle(ParsedMigrationName name)
        {
            return name.Type == "StoredProcedure";
        }
    }
}
