using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace StockService;

[DependsOn(
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(StockServiceDomainModule)
)]
public class StockServiceEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<StockServiceDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });
    }
}
