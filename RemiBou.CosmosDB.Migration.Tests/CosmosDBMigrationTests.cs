using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace RemiBou.CosmosDB.Migration.Tests
{
    public class CosmosDBMigrationTests : IDisposable
    {
        private DocumentClient documentClient;
        private IServiceProvider serviceCollection = new ServiceCollection().BuildServiceProvider();

        public CosmosDBMigrationTests()
        {

            documentClient = new DocumentClient(
             new Uri(Environment.GetEnvironmentVariable("CosmosDBEndpoint") ?? "https://localhost:8081"),
             "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");

        }
        [Fact]
        public async Task Migrate_CreatesDataBase()
        {
            await new CosmosDBMigration(documentClient, serviceCollection, null).MigrateAsync(this.GetType().Assembly);
            var db = await documentClient.ReadDatabaseAsync(UriFactory.CreateDatabaseUri("TestDataBase"));
            Assert.Equal(HttpStatusCode.OK, db.StatusCode);
        }
        [Fact]
        public async Task Migrate_CreatesCollection()
        {
            await new CosmosDBMigration(documentClient, serviceCollection, null).MigrateAsync(this.GetType().Assembly);
            var db = await documentClient.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri("TestDataBase", "TestCollection"));
            Assert.Equal(HttpStatusCode.OK, db.StatusCode);

        }
        [Fact]
        public async Task Migrate_CreatesStoredProcedure()
        {
            await new CosmosDBMigration(documentClient, serviceCollection, null).MigrateAsync(this.GetType().Assembly);
            var db = await documentClient.ReadStoredProcedureAsync(UriFactory.CreateStoredProcedureUri("TestDataBase", "TestCollection", "StoredProcedure2"));
            Assert.Equal(HttpStatusCode.OK, db.StatusCode);
        }

        [Fact]
        public async Task Migrate_CreatesTrigger()
        {

            await new CosmosDBMigration(documentClient, serviceCollection, null).MigrateAsync(this.GetType().Assembly);
            var db = await documentClient.ReadTriggerAsync(UriFactory.CreateTriggerUri("TestDataBase", "TestCollection", "updateMetadata"));
            Assert.Equal(HttpStatusCode.OK, db.StatusCode);
            Assert.Equal(TriggerType.Pre, db.Resource.TriggerType);
            Assert.Equal(TriggerOperation.Create, db.Resource.TriggerOperation);
        }

        [Fact]
        public async Task Migrate_CreatesUDF()
        {
            await new CosmosDBMigration(documentClient, serviceCollection, null).MigrateAsync(this.GetType().Assembly);
            var db = await documentClient.ReadUserDefinedFunctionAsync(UriFactory.CreateUserDefinedFunctionUri("TestDataBase", "TestCollection", "tax"));
            Assert.Equal(HttpStatusCode.OK, db.StatusCode);
        }

        [Fact]
        public async Task Migrate_UseFolderFromOptions()
        {
            await new CosmosDBMigration(documentClient, serviceCollection, Options.Create(new CosmosDBMigrationOptions()
            {
                MigrationFolder = "CosmosDBDifferentFolder.Migrations"
            })).MigrateAsync(this.GetType().Assembly);
            var db = await documentClient.ReadStoredProcedureAsync(UriFactory.CreateStoredProcedureUri("TestDataBase2", "TestCollection2", "StoredProcedure3"));
            Assert.Equal(HttpStatusCode.OK, db.StatusCode);
        }

        [Fact]
        public async Task Migrate_DoBulkImport()
        {
            await new CosmosDBMigration(documentClient, serviceCollection, Options.Create(new CosmosDBMigrationOptions()
            {
                MigrationFolder = "CosmosDBBulkImport.Migrations"
            })).MigrateAsync(this.GetType().Assembly);
            var db = await documentClient.ReadDocumentAsync(UriFactory.CreateDocumentUri("TestDataBase3", "TestCollection3", "3"));
            Assert.Equal(HttpStatusCode.OK, db.StatusCode);
        }

        [Fact]
        public async Task Migrate_DoBulkImportOnlyOnce()
        {
            await new CosmosDBMigration(documentClient, serviceCollection, Options.Create(new CosmosDBMigrationOptions()
            {
                MigrationFolder = "CosmosDBBulkImport.Migrations"
            })).MigrateAsync(this.GetType().Assembly);
            await documentClient.DeleteDocumentAsync(UriFactory.CreateDocumentUri("TestDataBase3", "TestCollection3", "3"));
            await new CosmosDBMigration(documentClient, serviceCollection, Options.Create(new CosmosDBMigrationOptions()
            {
                MigrationFolder = "CosmosDBBulkImport.Migrations"
            })).MigrateAsync(this.GetType().Assembly);

            try
            {
                var db = await documentClient.ReadDocumentAsync(UriFactory.CreateDocumentUri("TestDataBase3", "TestCollection3", "3"));
                throw new Exception("Failed test");
            }
            catch (DocumentClientException e)
            {
                Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
            }
        }

        public async Task Migrate_ShouldApplyMigrationInGivenOrder()
        {

        }

        public async Task Migrate_UpdateExecuted()
        {

        }

        public void Dispose()
        {
            try
            {
                var res = documentClient.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri("TestDataBase")).Result;
            }
            catch (Exception)
            {//ignore, the 
            }
        }
    }
}
