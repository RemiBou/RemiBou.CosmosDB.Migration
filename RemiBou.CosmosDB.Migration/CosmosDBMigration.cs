using AspNetCore.AsyncInitialization;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RemiBou.CosmosDB.Migration
{
    public class CosmosDBMigration : IAsyncInitializer
    {
        private List<IMigrationStrategy> strategies = new List<IMigrationStrategy>()
        {
            new StoredProcedureMigrationStrategy()
        };
        private readonly IDocumentClient client;
        private readonly IOptionsSnapshot<CosmosDBMigrationOption> optionsAccessor;

        public CosmosDBMigration(IDocumentClient client, IOptionsSnapshot<CosmosDBMigrationOption> optionsAccessor)
        {
            this.client = client;
            this.optionsAccessor = optionsAccessor;
        }

        public async Task InitializeAsync()
        {
            Database db = await client.CreateDatabaseIfNotExistsAsync(
                new Database() { Id = optionsAccessor.Value.DataBaseName });
            DocumentCollection collection = await client.CreateDocumentCollectionIfNotExistsAsync(
                db.SelfLink, new DocumentCollection() { Id = optionsAccessor.Value.MigrationCollectionName });
            //get all the already applied migrations
            var existingMigrations
                = client.CreateDocumentQuery<CosmosDBMigrationDocument>(collection.SelfLink).ToList();

            //read all the migration embed in CosmosDB/Migrations
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            var ressources = executingAssembly.GetManifestResourceNames()
                .Where(r => r.StartsWith("CosmosDB.Migrations") && r.EndsWith(".js"))
                .OrderBy(r => r)
                .ToList();
            //for each migration
            foreach (var migration in ressources)
            {
                string migrationContent;
                using (var stream = executingAssembly.GetManifestResourceStream(migration))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        migrationContent = await reader.ReadToEndAsync();
                    }
                }
                // if already done
                if (existingMigrations.Any(m => m.Id == migration))
                {
                    // check checksum and display warning
                }
                else
                {
                    var strategy = strategies.FirstOrDefault(s => s.Handle(migration));
                    if (strategy == null)
                    {
                        throw new InvalidOperationException(string.Format("No strategy found for migration '{0}", migration));
                    }
                    await strategy.ApplyMigrationAsync(client, migration, migrationContent);
                    var migrationDocument = new CosmosDBMigrationDocument(migration);
                    await client.CreateDocumentAsync(collection.SelfLink, migrationDocument);
                    //SP
                    //Trigger
                    //User
                    //Permission
                    //Bulk Update
                    //BUlk Insert

                    // applies migration
                    // insert it as done
                }

            }
            //end
            //for each applied mgration
            // if not in embed
            // display warning
        }
    }
}
