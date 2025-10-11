using Volo.Abp.Modularity;
using Volo.Abp.Domain;

namespace ProductService;

[DependsOn(
    typeof(AbpDddDomainModule)
)]
public class ProductServiceDomainModule : AbpModule
{
}
