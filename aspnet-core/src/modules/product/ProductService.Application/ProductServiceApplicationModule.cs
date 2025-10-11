using Volo.Abp.Modularity;
using Volo.Abp.Application;
using Volo.Abp.AutoMapper;

namespace ProductService;

[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(AbpAutoMapperModule),
    typeof(ProductServiceApplicationContractsModule),
    typeof(ProductServiceDomainModule)
)]
public class ProductServiceApplicationModule : AbpModule
{
}
