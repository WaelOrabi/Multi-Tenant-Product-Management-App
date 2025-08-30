using Volo.Abp.Modularity;

namespace MultiTenantProductManagementApp;

/* Inherit from this class for your domain layer tests. */
public abstract class MultiTenantProductManagementAppDomainTestBase<TStartupModule> : MultiTenantProductManagementAppTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
