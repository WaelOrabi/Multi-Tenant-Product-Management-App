using Volo.Abp.Modularity;
using Volo.Abp.EntityFrameworkCore;

namespace MultiTenantProductManagementApp.Products;

[DependsOn(
    typeof(AbpEntityFrameworkCoreModule),
    typeof(ProductsDomainModule)
)]
public class ProductsEntityFrameworkCoreModule : AbpModule
{
}
