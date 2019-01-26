using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace RemiBou.CosmosDB.Migration
{
    /// <summary>
    /// Parsed migration ressource name
    /// </summary>
    public class ParsedMigrationName
    {
        private const string Prefix = "CosmosDB.Migrations.";

        /// <summary>
        /// Full ressource name without the prefix : "{Top namespace}.CosmosDb.Migrations"
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// Type : detected migration type
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// If in a subfolder of database then it's the collection name
        /// </summary>
        public string CollectionName { get; }

        /// <summary>
        /// Object version of the collection, null if none specified
        /// </summary>
        public DocumentCollection Collection { get; }

        /// <summary>
        /// Database name
        /// </summary>
        public string DataBaseName { get; }

        /// <summary>
        /// Object version of the database
        /// </summary>
        public Database DataBase { get; }

        /// <summary>
        /// Migration name, can be parsed by strategy
        /// </summary>
        public string Name { get; }

        public ParsedMigrationName(string fullMigrationName)
        {
            //name should be formated like that 
            //"TpNameSpace.CosmosDB.Migrations.{DataBaseName}.{collection?}.{type}.{name}.js"
            FullName = fullMigrationName.Substring(fullMigrationName.IndexOf(Prefix) + Prefix.Length);
            var split = FullName.Split('.');
            DataBaseName = split[0];
            DataBase = new Database() { Id = DataBaseName };
            //if length equals 5 then there is a collection name
            if (split.Length == 4)
            {
                Type = split[1];
                Name = split[2];
            }
            else
            {
                CollectionName = split[1];
                Collection = new DocumentCollection() { Id = CollectionName };
                Type = split[2];
                Name = split[3];
            }
        }
    }
}
