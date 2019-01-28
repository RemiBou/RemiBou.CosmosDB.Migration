using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace RemiBou.CosmosDB.Migration.Tests
{
    public class CosmosDBMigrationTests : IDisposable
    {
        private DocumentClient documentClient;

        public CosmosDBMigrationTests()
        {
            documentClient = new DocumentClient(
             new Uri("https://localhost:8081"),
             "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");

        }
        [Fact]
        public async Task Migrate_CreatesDataBase()
        {
            await new CosmosDBMigration(documentClient).MigrateAsync(this.GetType().Assembly);
            var db = await documentClient.ReadDatabaseAsync(UriFactory.CreateDatabaseUri("TestDataBase"));
            Assert.Equal(HttpStatusCode.OK, db.StatusCode);
        }
        [Fact]
        public async Task Migrate_CreatesCollection()
        {
            await new CosmosDBMigration(documentClient).MigrateAsync(this.GetType().Assembly);
            var db = await documentClient.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri("TestDataBase", "TestCollection"));
            Assert.Equal(HttpStatusCode.OK, db.StatusCode);

        }
        [Fact]
        public async Task Migrate_CreatesStoredProcedure()
        {
            await new CosmosDBMigration(documentClient).MigrateAsync(this.GetType().Assembly);
            var db = await documentClient.ReadStoredProcedureAsync(UriFactory.CreateStoredProcedureUri("TestDataBase", "TestCollection", "StoredProcedure2"));
            Assert.Equal(HttpStatusCode.OK, db.StatusCode);
        }

        [Fact]
        public async Task Migrate_CreatesTrigger()
        {

            await new CosmosDBMigration(documentClient).MigrateAsync(this.GetType().Assembly);
            var db = await documentClient.ReadTriggerAsync(UriFactory.CreateTriggerUri("TestDataBase", "TestCollection", "updateMetadata"));
            Assert.Equal(HttpStatusCode.OK, db.StatusCode);
            Assert.Equal(TriggerType.Pre, db.Resource.TriggerType);
            Assert.Equal(TriggerOperation.Create, db.Resource.TriggerOperation);
        }

        [Fact]
        public async Task Migrate_CreatesUDF()
        {
            await new CosmosDBMigration(documentClient).MigrateAsync(this.GetType().Assembly);
            var db = await documentClient.ReadUserDefinedFunctionAsync(UriFactory.CreateUserDefinedFunctionUri("TestDataBase", "TestCollection", "tax"));
            Assert.Equal(HttpStatusCode.OK, db.StatusCode);
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
