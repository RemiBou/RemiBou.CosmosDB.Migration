﻿
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
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

        private List<IScriptMigrationStrategy> scriptStrategies = new List<IScriptMigrationStrategy>()
        {
            new StoredProcedureMigrationStrategy(),
            new TriggerMigrationStrategy(),
            new FunctionMigrationStrategy()
        };
        private List<IClassMigrationStrategy> classStrategies;
        private readonly BulkMigrationExecutions bulkPreviousExecutions;
        private readonly DocumentClient client;
        private readonly IServiceProvider serviceProvider;
        private readonly IOptions<CosmosDBMigrationOptions> options;

        /// <summary>
        /// Build the migrator
        /// </summary>
        /// <param name="client">CosmosDB Client</param>
        public CosmosDBMigration(DocumentClient client, IServiceProvider serviceProvider, IOptions<CosmosDBMigrationOptions> options)
        {
            this.bulkPreviousExecutions = new BulkMigrationExecutions();
            classStrategies = new List<IClassMigrationStrategy>(){
                new BulkMigrationStrategy(bulkPreviousExecutions)
            };
            this.client = client;
            this.serviceProvider = serviceProvider;
            this.options = options ?? Options.Create(new CosmosDBMigrationOptions());
        }

        /// <summary>
        /// Parse the migrations and apply them to the database
        /// </summary>
        /// <param name="migrationAssembly">The assembly where the migration are embedded</param>
        /// <returns></returns>
        public async Task MigrateAsync(Assembly migrationAssembly)
        {
            await ExecuteScriptStrategiesAsync(migrationAssembly);
            await ExecuteClassStrategiesAsync(migrationAssembly);
        }

        private async Task ExecuteClassStrategiesAsync(Assembly migrationAssembly)
        {
            var migrations = migrationAssembly.DefinedTypes
                .Where(t => t.Namespace != null)
                .Where(t => t.Namespace.Contains(this.options.Value.MigrationFolder))
                .Where(t => typeof(IClassMigration).IsAssignableFrom(t))
                .OrderBy(t => t.Name)
                .Select(t => t.AsType())
                .ToList();
            foreach (var migration in migrations)
            {
                var strategy = classStrategies.First(s => s.Handle(migration));
                if (strategy == null)
                {
                    throw new InvalidOperationException(string.Format("No strategy found for migration '{0}", migration.GetType()));
                }
                var ns = migration.Namespace;
                var nameSplit = ns.Substring(ns.IndexOf(options.Value.MigrationFolder) + options.Value.MigrationFolder.Length + 1).Split('.');
                await client.CreateDatabaseIfNotExistsAsync(new Database() { Id = nameSplit[0] });

                await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(nameSplit[0]), new DocumentCollection() { Id = nameSplit[1] });

                await strategy.ApplyMigrationAsync(client, nameSplit[0], nameSplit[1], ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider,migration));
            }
        }

        private async Task ExecuteScriptStrategiesAsync(Assembly migrationAssembly)
        {
            //read all the migration embed in CosmosDB/Migrations
            var ressources = migrationAssembly.GetManifestResourceNames()
                .Where(r => r.Contains("." + options.Value.MigrationFolder + "."))
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
                var strategy = scriptStrategies.FirstOrDefault(s => s.Handle(parsedMigration));
                if (strategy == null)
                {
                    throw new InvalidOperationException(string.Format("No strategy found for migration '{0}", migration));
                }

                await client.CreateDatabaseIfNotExistsAsync(parsedMigration.DataBase);

                await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(parsedMigration.DataBase.Id), parsedMigration.Collection);


                await strategy.ApplyMigrationAsync(client, parsedMigration, migrationContent);
            }
        }

        /// <summary>
        /// Add a custom strategy for migrating
        /// </summary>
        /// <param name="strategy"></param>
        public void AddStrategy(IScriptMigrationStrategy strategy)
        {
            this.scriptStrategies.Add(strategy);
        }
    }
}
