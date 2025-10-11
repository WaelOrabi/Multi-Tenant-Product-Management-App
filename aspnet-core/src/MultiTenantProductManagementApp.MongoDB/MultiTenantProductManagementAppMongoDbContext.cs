using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using MongoDB.Driver;

namespace MultiTenantProductManagementApp;

[ConnectionStringName("Default")]
public class MultiTenantProductManagementAppMongoDbContext : AbpMongoDbContext
{
    protected override void CreateModel(IMongoModelBuilder builder)
    {
        base.CreateModel(builder);
    }
}
