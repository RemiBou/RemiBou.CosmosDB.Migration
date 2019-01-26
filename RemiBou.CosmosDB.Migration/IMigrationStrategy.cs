using Microsoft.Azure.Documents;
using System.Threading.Tasks;

namespace RemiBou.CosmosDB.Migration
{
    /// <summary>
    /// Common interface for all the migration strategies
    /// </summary>
    public interface IMigrationStrategy
    {
        /// <summary>
        /// Returns true if this migration should be handled by this strategy, else false
        /// </summary>
        /// <param name="migration"></param>
        /// <returns></returns>
        bool Handle(ParsedMigrationName migration);

        /// <summary>
        /// Applies the given migration
        /// </summary>
        /// <param name="client"></param>
        /// <param name="migration"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        Task ApplyMigrationAsync(IDocumentClient client, ParsedMigrationName migration, string content);
    }
}
