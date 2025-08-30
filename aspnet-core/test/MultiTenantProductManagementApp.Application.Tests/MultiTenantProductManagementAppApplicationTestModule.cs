using Volo.Abp.Modularity;

namespace MultiTenantProductManagementApp;

[DependsOn(
    typeof(MultiTenantProductManagementAppApplicationModule),
    typeof(MultiTenantProductManagementAppDomainTestModule)
)]
public class MultiTenantProductManagementAppApplicationTestModule : AbpModule
{

}
