using Microsoft.Azure.Documents;
using System.Threading.Tasks;

namespace RemiBou.CosmosDB.Migration
{
    public interface IMigrationStrategy
    {
        bool Handle(string name);
        Task ApplyMigrationAsync(IDocumentClient client, ParsedMigrationName name, string content);
    }
}
