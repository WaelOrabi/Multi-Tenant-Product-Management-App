using Volo.Abp.Modularity;

namespace MultiTenantProductManagementApp;

[DependsOn(
    typeof(MultiTenantProductManagementAppDomainModule),
    typeof(MultiTenantProductManagementAppTestBaseModule)
)]
public class MultiTenantProductManagementAppDomainTestModule : AbpModule
{

}
