using Volo.Abp.Modularity;
using Volo.Abp.Domain;

namespace StockService;

[DependsOn(
    typeof(AbpDddDomainModule)
)]
public class StockServiceDomainModule : AbpModule
{
}
