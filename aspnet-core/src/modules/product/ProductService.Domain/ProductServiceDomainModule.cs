using Volo.Abp.Modularity;
using Volo.Abp.Domain;
using Volo.Abp.TenantManagement;
using Volo.Abp.Identity;
using Volo.Abp.Settings;
using MultiTenantProductManagementApp.Products.Settings;

namespace ProductService;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(AbpTenantManagementDomainModule),
    typeof(AbpIdentityDomainModule)
)]

public class ProductServiceDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpSettingOptions>(options =>
        {
            options.DefinitionProviders.Add<ProductSettingDefinitionProvider>();
        });
    }
}
