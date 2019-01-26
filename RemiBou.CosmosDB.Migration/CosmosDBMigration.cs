
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RemiBou.CosmosDB.Migration
{
    public class CosmosDBMigration
    {
        private List<IMigrationStrategy> strategies = new List<IMigrationStrategy>()
        {
            new StoredProcedureMigrationStrategy(),
            new TriggerMigrationStrategy(),
            new FunctionMigrationStrategy()
        };
        private readonly IDocumentClient client;

        public CosmosDBMigration(IDocumentClient client)
        {
            this.client = client;
        }

        public async Task Migrate(Assembly migrationAssembly)
        {


            //read all the migration embed in CosmosDB/Migrations
            var ressources = migrationAssembly.GetManifestResourceNames()
                .Where(r => r.Contains(".CosmosDB.Migrations.") && r.EndsWith(".js"))
                .OrderBy(r => r)
                .ToList();
            //for each migration
            foreach (var migration in ressources)
            {
                string migrationContent;
                using (var stream = migrationAssembly.GetManifestResourceStream(migration))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        migrationContent = await reader.ReadToEndAsync();
                    }
                }
                var parsedMigration = new ParsedMigrationName(migration);
                var strategy = strategies.FirstOrDefault(s => s.Handle(parsedMigration));
                if (strategy == null)
                {
                    throw new InvalidOperationException(string.Format("No strategy found for migration '{0}", migration));
                }

                await client.CreateDatabaseIfNotExistsAsync(parsedMigration.DataBase);
                if (parsedMigration.Collection != null)
                {
                    await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(parsedMigration.DataBase.Id), parsedMigration.Collection);
                }

                await strategy.ApplyMigrationAsync(client, parsedMigration, migrationContent);

                //SP
                //Trigger
                //User
                //Permission
                // later : 
                //Bulk Update
                //BUlk Insert


            }
        }
    }
}
