using MultiTenantProductManagementApp.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace MultiTenantProductManagementApp.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(MultiTenantProductManagementAppEntityFrameworkCoreModule),
    typeof(MultiTenantProductManagementAppApplicationContractsModule)
    )]
public class MultiTenantProductManagementAppDbMigratorModule : AbpModule
{
}
