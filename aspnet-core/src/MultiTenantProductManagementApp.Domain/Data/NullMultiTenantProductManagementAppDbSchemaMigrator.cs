using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace MultiTenantProductManagementApp.Data;

/* This is used if database provider does't define
 * IMultiTenantProductManagementAppDbSchemaMigrator implementation.
 */
public class NullMultiTenantProductManagementAppDbSchemaMigrator : IMultiTenantProductManagementAppDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
