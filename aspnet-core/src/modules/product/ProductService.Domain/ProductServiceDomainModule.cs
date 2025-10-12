using Volo.Abp.Modularity;
using Volo.Abp.Domain;
using Volo.Abp.TenantManagement;
using Volo.Abp.Identity;

namespace ProductService;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(AbpTenantManagementDomainModule),
    typeof(AbpIdentityDomainModule)
)]

public class ProductServiceDomainModule : AbpModule
{
}
