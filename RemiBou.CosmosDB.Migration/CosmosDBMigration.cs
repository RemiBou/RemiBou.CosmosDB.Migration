
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
    /// <summary>
    /// Entry point for launching the migrations
    /// </summary>
    public class CosmosDBMigration : ICosmosDBMigration
    {
        private List<IMigrationStrategy> strategies = new List<IMigrationStrategy>()
        {
            new StoredProcedureMigrationStrategy(),
            new TriggerMigrationStrategy(),
            new FunctionMigrationStrategy(),
            new BulkMigrationStrategy()
        };
        private readonly DocumentClient client;
        private readonly IOptions<CosmosDBMigrationOptions> options;

        /// <summary>
        /// Build the migrator
        /// </summary>
        /// <param name="client">CosmosDB Client</param>
        public CosmosDBMigration(DocumentClient client, IOptions<CosmosDBMigrationOptions> options)
        {
            this.client = client;
            this.options = options ?? Options.Create(new CosmosDBMigrationOptions());
        }

        /// <summary>
        /// Parse the migrations and apply them to the database
        /// </summary>
        /// <param name="migrationAssembly">The assembly where the migration are embedded</param>
        /// <returns></returns>
        public async Task MigrateAsync(Assembly migrationAssembly)
        {


            //read all the migration embed in CosmosDB/Migrations
            var ressources = migrationAssembly.GetManifestResourceNames()
                .Where(r => r.Contains("."+options.Value.ResourceFolder+"."))
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
                var parsedMigration = new ParsedMigrationName(migration, options);
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

        /// <summary>
        /// Add a custom strategy for migrating
        /// </summary>
        /// <param name="strategy"></param>
        public void AddStrategy(IMigrationStrategy strategy)
        {
            this.strategies.Add(strategy);
        }
    }
}
