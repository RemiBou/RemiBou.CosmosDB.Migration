using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemiBou.CosmosDB.Migration
{
    internal class BulkMigrationExecutions
    {
        private static PreviousBulkContainer previousExecutions;
        public async Task<bool> CanExecuteAsync(DocumentClient client, string databaseName, string collectionName, string migrationName)
        {
            if (previousExecutions == null)
            {
                try
                {
                    previousExecutions = await client.ReadDocumentAsync<PreviousBulkContainer>(UriFactory.CreateDocumentUri(databaseName, collectionName, "BulkExecutions"));
                }
                catch (DocumentClientException e)
                {
                    if (e.StatusCode != System.Net.HttpStatusCode.NotFound)
                        throw;
                    previousExecutions = new PreviousBulkContainer();
                }
            }
            return !previousExecutions.Executions.Contains(migrationName);
        }
        private class PreviousBulkContainer
        {
            public HashSet<string> Executions { get; set; } = new HashSet<string>();
            public string Id { get; set; } = "BulkExecutions";
        }

        internal async Task RegisterExecutedAsync(DocumentClient client, string databaseName, string collectionName, string name)
        {
           
            previousExecutions.Executions.Add(name);
            await client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), previousExecutions);
        }
    }
}
