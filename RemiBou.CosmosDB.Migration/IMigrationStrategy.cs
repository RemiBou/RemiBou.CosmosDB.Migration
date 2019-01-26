using Microsoft.Azure.Documents;
using System.Threading.Tasks;

namespace RemiBou.CosmosDB.Migration
{
    public interface IMigrationStrategy
    {
        bool Handle(ParsedMigrationName migration);
        Task ApplyMigrationAsync(IDocumentClient client, ParsedMigrationName migration, string content);
    }
}
