using Volo.Abp.Modularity;
using Volo.Abp;
using Volo.Abp.Domain;

namespace MultiTenantProductManagementApp.Products;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(ProductsDomainSharedModule)
)]
public class ProductsDomainModule : AbpModule
{
}
