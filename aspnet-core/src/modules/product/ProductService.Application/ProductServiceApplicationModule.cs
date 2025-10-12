using Volo.Abp.Modularity;
using Volo.Abp.Application;
using Volo.Abp.AutoMapper;
using Volo.Abp.FluentValidation;

namespace ProductService;

[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(AbpAutoMapperModule),
    typeof(ProductServiceApplicationContractsModule),
    typeof(ProductServiceDomainModule),
    typeof(AbpFluentValidationModule)
)]
public class ProductServiceApplicationModule : AbpModule
{
}
