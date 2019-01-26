using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Threading.Tasks;

namespace RemiBou.CosmosDB.Migration
{
    internal class FunctionMigrationStrategy : IMigrationStrategy
    {
        public async Task ApplyMigrationAsync(IDocumentClient client, ParsedMigrationName migration, string content)
        {            
            await client.UpsertUserDefinedFunctionAsync(
                UriFactory.CreateDocumentCollectionUri(migration.DataBase.Id, migration.Collection.Id),
                new UserDefinedFunction()
                {
                    Body = content,                    
                    Id = migration.Name
                });
        }

        public bool Handle(ParsedMigrationName name)
        {
            return name.Type == "Function";
        }
    }
}
