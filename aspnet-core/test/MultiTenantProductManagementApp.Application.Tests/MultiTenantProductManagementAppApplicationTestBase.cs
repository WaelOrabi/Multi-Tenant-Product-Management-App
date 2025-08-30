using Volo.Abp.Modularity;

namespace MultiTenantProductManagementApp;

public abstract class MultiTenantProductManagementAppApplicationTestBase<TStartupModule> : MultiTenantProductManagementAppTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
