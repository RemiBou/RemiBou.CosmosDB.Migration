using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkUpdate;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemiBou.CosmosDB.Migration
{
    internal class BulkMigrationStrategy : IMigrationStrategy
    {
        private static PreviousBulkContainer previousExecutions;
        public async Task ApplyMigrationAsync(DocumentClient client, ParsedMigrationName migration, string content)
        {
            if (previousExecutions == null)
            {
                try
                {
                    previousExecutions = await client.ReadDocumentAsync<PreviousBulkContainer>(UriFactory.CreateDocumentUri(migration.DataBaseName, migration.CollectionName, "BulkExecutions"));
                }
                catch (DocumentClientException e)
                {
                    if (e.StatusCode != System.Net.HttpStatusCode.NotFound)
                        throw;
                    previousExecutions = new PreviousBulkContainer();
                }
            }
            if (previousExecutions.Executions.Contains(migration.Name))
            {
                return;
            }
            UnsetUpdateOperation
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

            previousExecutions.Executions.Add(migration.Name);
            await client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(migration.DataBaseName, migration.CollectionName), previousExecutions);
        }
        private class PreviousBulkContainer
        {
            public HashSet<string> Executions { get; set; } = new HashSet<string>();
            public string Id { get; set; } = "BulkExecutions";
        }
        public bool Handle(ParsedMigrationName migration)
        {
            return migration.Type == "Bulk";
        }


    }
}
