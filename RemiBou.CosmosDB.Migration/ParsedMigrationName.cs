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
        public string DataBaseName { get; }
        public string Name { get; }

        public ParsedMigrationName(string fullMigrationName)
        {
            //name should be formated like that 
            //"CosmosDB.Migrations.{DataBaseName}.{collection?}.{type}.{name}.js"
            FullName = fullMigrationName.Substring(fullMigrationName.IndexOf(Prefix) + Prefix.Length);
            var split = FullName.Split('.');
            DataBaseName = split[0];
            //if length equals 5 then there is a collection name
            if (split.Length == 5)
            {
                Type = split[3];
                Name = split[4];
            }
            else
            {
                CollectionName = split[3];
                Type = split[4];
                Name = split[5];
            }
        }
    }
}
