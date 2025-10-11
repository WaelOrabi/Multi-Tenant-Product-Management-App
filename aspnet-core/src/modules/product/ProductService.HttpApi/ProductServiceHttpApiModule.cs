using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace ProductService;

[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(ProductServiceApplicationModule)
)]
public class ProductServiceHttpApiModule : AbpModule
{
}
