using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Threading.Tasks;

namespace RemiBou.CosmosDB.Migration
{
    public class TriggerMigrationStrategy : IMigrationStrategy
    {
        public async Task ApplyMigrationAsync(IDocumentClient client, ParsedMigrationName migration, string content)
        {
            var nameSplit = migration.Name.Split('-');

            Trigger trigger = new Trigger()
            {

                Body = content,
                Id = nameSplit[2],
                TriggerOperation = (TriggerOperation)Enum.Parse(typeof(TriggerOperation), nameSplit[1]),
                TriggerType = (TriggerType)Enum.Parse(typeof(TriggerType), nameSplit[0])
            };
            await client.UpsertTriggerAsync(
                           UriFactory.CreateDocumentCollectionUri(migration.DataBase.Id, migration.Collection.Id),
                           trigger);
        }

        public bool Handle(ParsedMigrationName migration)
        {
            return migration.Type == "Trigger";
        }
    }
}
