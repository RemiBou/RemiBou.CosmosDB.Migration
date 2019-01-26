using System.Reflection;
using System.Threading.Tasks;

namespace RemiBou.CosmosDB.Migration
{
    public interface ICosmosDBMigration
    {
        Task MigrateAsync(Assembly migrationAssembly);
    }
}