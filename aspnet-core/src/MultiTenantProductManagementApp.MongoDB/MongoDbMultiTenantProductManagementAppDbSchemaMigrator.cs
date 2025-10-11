using System;
using System.Threading.Tasks;
using MultiTenantProductManagementApp.Data;
using Volo.Abp.DependencyInjection;

namespace MultiTenantProductManagementApp;

public class MongoDbMultiTenantProductManagementAppDbSchemaMigrator : IMultiTenantProductManagementAppDbSchemaMigrator, ITransientDependency
{
    private readonly MultiTenantProductManagementAppMongoDbContext _mongoContext;

    public MongoDbMultiTenantProductManagementAppDbSchemaMigrator(MultiTenantProductManagementAppMongoDbContext mongoContext)
    {
        _mongoContext = mongoContext;
    }

    public async Task MigrateAsync()
    {
        await Task.CompletedTask;
    }
}
