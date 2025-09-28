using Volo.Abp.Modularity;
using Volo.Abp;
using Volo.Abp.Application;

namespace MultiTenantProductManagementApp.Products;

[DependsOn(
    typeof(AbpDddApplicationContractsModule),
    typeof(ProductsDomainSharedModule)
)]
public class ProductsApplicationContractsModule : AbpModule
{
}
