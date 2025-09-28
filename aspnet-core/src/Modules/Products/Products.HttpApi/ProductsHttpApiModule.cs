using Volo.Abp.Modularity;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp;

namespace MultiTenantProductManagementApp.Products;

[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(ProductsApplicationModule),
    typeof(ProductsApplicationContractsModule)
)]
public class ProductsHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(ProductsApplicationModule).Assembly);
        });
    }
}
