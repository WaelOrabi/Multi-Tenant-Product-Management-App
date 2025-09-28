using Volo.Abp.Modularity;
using Volo.Abp;
using Volo.Abp.Application;
using Volo.Abp.AutoMapper;

namespace MultiTenantProductManagementApp.Products;

[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(AbpAutoMapperModule),
    typeof(ProductsApplicationContractsModule),
    typeof(ProductsDomainModule)
)]
public class ProductsApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<ProductsApplicationModule>(validate: true);
        });
    }
}
