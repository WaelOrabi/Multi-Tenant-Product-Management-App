using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;
using Volo.Abp.UI.Navigation;
namespace MultiTenantProductManagementApp;

[DependsOn(
    typeof(MultiTenantProductManagementAppDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(MultiTenantProductManagementAppApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
 
    )]
public class MultiTenantProductManagementAppApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<MultiTenantProductManagementAppApplicationModule>();
        });
             Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new MultiTenantProductManagementAppMenuContributor());
        });
        
    }
}
