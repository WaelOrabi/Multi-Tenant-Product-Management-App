using System;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;
using MultiTenantProductManagementApp.Shared;
using Volo.Abp.Uow;
using MultiTenantProductManagementApp.Products;
using MultiTenantProductManagementApp.Stocks;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.MongoDB;
using Volo.Abp.Auditing;
using Volo.Abp.AuditLogging.MongoDB;

namespace MultiTenantProductManagementApp.MongoDB;

[DependsOn(
    typeof(MultiTenantProductManagementAppApplicationTestModule),
    typeof(MultiTenantProductManagementAppMongoDbModule),
    typeof(AbpTenantManagementMongoDbModule),
    typeof(AbpAuditLoggingMongoDbModule)
)]
public class MultiTenantProductManagementAppMongoDbTestModule : AbpModule
{
    private string _dbName = "MultiTenantProductManagementApp_Tests_Mongo";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Disable UoW transactions for tests
        context.Services.AddAlwaysDisableUnitOfWorkTransaction();

        // Disable auditing to avoid requiring IAuditLogRepository in tests
        Configure<AbpAuditingOptions>(options =>
        {
            options.IsEnabled = false;
        });

        // Allow overriding DB name via env var
        var envName = Environment.GetEnvironmentVariable("MONGO_TEST_DB_NAME");
        if (!string.IsNullOrWhiteSpace(envName))
        {
            _dbName = envName!;
        }

        var conn = Environment.GetEnvironmentVariable("MONGO_TEST_CONN") ?? "mongodb://localhost:27017";

        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = $"{conn}/{_dbName}";
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var resetEnv = Environment.GetEnvironmentVariable("RESET_TEST_DB");
        var resetDb = !string.IsNullOrWhiteSpace(resetEnv) && (resetEnv.Equals("1") || resetEnv.Equals("true", StringComparison.OrdinalIgnoreCase));

        if (resetDb)
        {
            var conn = Environment.GetEnvironmentVariable("MONGO_TEST_CONN") ?? "mongodb://localhost:27017";
            var client = new global::MongoDB.Driver.MongoClient(conn);
            client.DropDatabase(_dbName);
        }

        using var scope = context.ServiceProvider.CreateScope();
        // Ensure test admin exists (idempotent). In Mongo tests, Identity stores may not be wired;
        // swallow any DI resolution errors to avoid failing test startup.
        try
        {
            TestAdminSeeder.EnsureAdminAsync(scope.ServiceProvider).GetAwaiter().GetResult();
        }
        catch
        {
            // Intentionally ignored for Mongo test environment
        }
    }
}
