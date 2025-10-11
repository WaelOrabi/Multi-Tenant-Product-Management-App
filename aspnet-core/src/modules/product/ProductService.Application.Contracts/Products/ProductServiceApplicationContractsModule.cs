using Volo.Abp.Modularity;
using Volo.Abp.Application;

namespace ProductService;

[DependsOn(
    typeof(AbpDddApplicationContractsModule)
)]
public class ProductServiceApplicationContractsModule : AbpModule
{
}
