using Volo.Abp.Modularity;
using Volo.Abp.Application;
using Volo.Abp.AutoMapper;

namespace StockService;

[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(AbpAutoMapperModule),
    typeof(StockServiceApplicationContractsModule),
    typeof(StockServiceDomainModule)
)]
public class StockServiceApplicationModule : AbpModule
{
}
