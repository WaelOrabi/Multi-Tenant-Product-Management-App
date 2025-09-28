using Volo.Abp.Modularity;
using Volo.Abp;
using Volo.Abp.Domain;

namespace MultiTenantProductManagementApp.Products;

[DependsOn(
    typeof(AbpDddDomainSharedModule)
)]
public class ProductsDomainSharedModule : AbpModule
{
}
