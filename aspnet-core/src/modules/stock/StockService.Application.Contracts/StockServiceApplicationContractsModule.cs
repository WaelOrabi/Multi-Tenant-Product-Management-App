using Volo.Abp.Modularity;
using Volo.Abp.Application;

namespace StockService;

[DependsOn(
    typeof(AbpDddApplicationContractsModule)
)]
public class StockServiceApplicationContractsModule : AbpModule
{
}
