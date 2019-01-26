using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace RemiBou.CosmosDB.Migration
{
    public class ParsedMigrationName
    {
        private const string Prefix = "CosmosDB.Migrations.";

        public string FullName { get; }
        public string Type { get; }
        public string CollectionName { get; }
        public DocumentCollection Collection
        {
            get;
        }
        public string DataBaseName { get; }

        public Database DataBase { get; }
        public string Name { get; }

        public ParsedMigrationName(string fullMigrationName)
        {
            //name should be formated like that 
            //"CosmosDB.Migrations.{DataBaseName}.{collection?}.{type}.{name}.js"
            FullName = fullMigrationName.Substring(fullMigrationName.IndexOf(Prefix) + Prefix.Length);
            var split = FullName.Split('.');
            DataBaseName = split[0];
            DataBase = new Database() { Id = DataBaseName };
            //if length equals 5 then there is a collection name
            if (split.Length == 3)
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
