using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace RemiBou.CosmosDB.Migration
{
    internal class BulkMigrationStrategy : IMigrationStrategy
    {
        public async Task ApplyMigrationAsync(DocumentClient client, ParsedMigrationName migration, string content)
        {
            // Set retry options high during initialization (default values).
            client.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 30;
            client.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 9;

            var collection = await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(migration.DataBaseName, migration.CollectionName));
            IBulkExecutor bulkExecutor = new BulkExecutor(client, collection);
            await bulkExecutor.InitializeAsync();

            // Set retries to 0 to pass complete control to bulk executor.
            client.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 0;
            client.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 0;
            var jsons = JArray.Parse(content).Select(v => v.ToString());
            await bulkExecutor.BulkImportAsync(
                   documents: jsons,
                   enableUpsert: true,
                   disableAutomaticIdGeneration: true,
                   maxConcurrencyPerPartitionKeyRange: null,
                   maxInMemorySortingBatchSize: null,
                   cancellationToken: new System.Threading.CancellationToken());
        }

        public bool Handle(ParsedMigrationName migration)
        {
            return migration.Type == "Bulk";
        }
    }
}
