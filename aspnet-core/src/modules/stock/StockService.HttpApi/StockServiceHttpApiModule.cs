using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace StockService;

[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(StockServiceApplicationModule)
)]
public class StockServiceHttpApiModule : AbpModule
{
}
