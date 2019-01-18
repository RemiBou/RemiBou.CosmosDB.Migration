using Microsoft.Azure.Documents;
using System;
using System.Threading.Tasks;

namespace RemiBou.CosmosDB.Migration
{
    public class StoredProcedureMigrationStrategy : IMigrationStrategy
    {
        public async Task ApplyMigrationAsync(IDocumentClient client, ParsedMigrationName name, string content)
        {
            await client.UpsertStoredProcedureAsync(
                "/dbs/" + name.DataBaseName + "/colls/" + name.CollectionName,
                new StoredProcedure()
                {
                    Body = content,
                    Id = name.Name
                });
        }

        public bool Handle(string name)
        {
            return name.Contains("_SP_");
        }
    }
}
