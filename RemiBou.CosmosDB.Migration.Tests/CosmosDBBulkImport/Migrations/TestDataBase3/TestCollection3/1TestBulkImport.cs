using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RemiBou.CosmosDB.Migration;
namespace RemiBou.CosmosDB.Migration.Tests.CosmosDBBulkImport.Migrations.TestDataBase3.TestCollection3
{
    public class _1TestBulkImport : IBulkImportMigration
    {
        public async Task<IEnumerable<object>> GetDocuments()
        {
            return new List<object>()
            {
                new{id = "3", name="test"}
            };
        
        }
    }
}
