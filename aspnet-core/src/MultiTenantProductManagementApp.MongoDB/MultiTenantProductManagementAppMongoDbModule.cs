using System;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;
using MultiTenantProductManagementApp.Products;
using MultiTenantProductManagementApp.Stocks;

namespace MultiTenantProductManagementApp;

[DependsOn(
    typeof(AbpMongoDbModule),
    typeof(MultiTenantProductManagementAppDomainModule)
)]
public class MultiTenantProductManagementAppMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<MultiTenantProductManagementAppMongoDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });
    }
}
