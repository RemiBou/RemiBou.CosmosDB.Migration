using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.Documents.Client;
using System;
using System.Threading.Tasks;

namespace RemiBou.CosmosDB.Migration
{

    internal class BulkMigrationStrategy : IClassMigrationStrategy
    {
        private readonly BulkMigrationExecutions executions;

        public BulkMigrationStrategy(BulkMigrationExecutions executions)
        {
            this.executions = executions;
        }
        public async Task ApplyMigrationAsync(DocumentClient client, string databaseName, string collectionName, object migration)
        {
            if (!await executions.CanExecuteAsync(client, databaseName, collectionName, migration.GetType().Name))
                return;
            var bulkMigration = migration as IBulkImportMigration;
            // Set retry options high during initialization (default values).
            client.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 30;
            client.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 9;

            var collection = await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName));
            IBulkExecutor bulkExecutor = new BulkExecutor(client, collection);
            await bulkExecutor.InitializeAsync();

            // Set retries to 0 to pass complete control to bulk executor.
            client.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 0;
            client.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 0;
            await bulkExecutor.BulkImportAsync(
                   documents: await bulkMigration.GetDocuments(),
                   enableUpsert: true,
                   disableAutomaticIdGeneration: true,
                   maxConcurrencyPerPartitionKeyRange: null,
                   maxInMemorySortingBatchSize: null,
                   cancellationToken: new System.Threading.CancellationToken());
            await executions.RegisterExecutedAsync(client, databaseName, collectionName, migration.GetType().Name);
        }

        public bool Handle(Type migrationType)
        {
            return typeof(IBulkImportMigration).IsAssignableFrom(migrationType);
        }


    }

    internal interface IClassMigrationStrategy
    {
        bool Handle(Type migrationType);
        Task ApplyMigrationAsync(DocumentClient client, string databaseName, string collectionName, object migration);
    }
}
