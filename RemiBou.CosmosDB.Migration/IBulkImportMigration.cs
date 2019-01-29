using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemiBou.CosmosDB.Migration
{
    public interface IClassMigration
    {

    }

    /// <summary>
    /// This interface needs to be implemented for applying bulk import
    /// </summary>
    public interface IBulkImportMigration : IClassMigration
    {
        /// <summary>
        /// The documents to import
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<object>> GetDocuments();
    }
}