using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantProductManagementApp.Data;
using Volo.Abp.DependencyInjection;

namespace MultiTenantProductManagementApp.EntityFrameworkCore;

public class EntityFrameworkCoreMultiTenantProductManagementAppDbSchemaMigrator
    : IMultiTenantProductManagementAppDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreMultiTenantProductManagementAppDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolve the MultiTenantProductManagementAppDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

            await _serviceProvider
            .GetRequiredService<MultiTenantProductManagementAppDbContext>()
            .Database
            .MigrateAsync();
    }
}
