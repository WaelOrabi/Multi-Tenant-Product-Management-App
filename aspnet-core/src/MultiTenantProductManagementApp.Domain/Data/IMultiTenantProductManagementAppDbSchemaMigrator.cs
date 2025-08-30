using System.Threading.Tasks;

namespace MultiTenantProductManagementApp.Data;

public interface IMultiTenantProductManagementAppDbSchemaMigrator
{
    Task MigrateAsync();
}
