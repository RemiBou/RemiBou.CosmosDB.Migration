using System;
using System.Collections.Generic;
using System.Text;

namespace RemiBou.CosmosDB.Migration
{
    public class ParsedMigrationName
    {
        public string FullName { get; }
        public string Type { get; }
        public string CollectionName { get; }
        public int VersionNumber { get; }
        public string DataBaseName { get; }
        public string Name { get; }

        public ParsedMigrationName(string fullMigrationName)
        {
            //name should be formated like that 
            //"CosmosDB.Migrations.{versionNumber}_{DataBaseName}_{type}_{collection?}_{name}.js"
            FullName = fullMigrationName;
            var split = FullName.Split('.');
            var fileName = split[split.Length - 2].Split('_');
            VersionNumber = int.Parse(fileName[0]);
            DataBaseName = fileName[1];
            Type = fileName[2];
            if (fileName.Length == 4)
            {
                Name = fileName[3];
            }
            else
            {
                CollectionName = fileName[3];
                Name = fileName[4];
            }
        }
    }
}
