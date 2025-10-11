using System;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.Modularity;
using ProductService;
using StockService;
using Volo.Abp.Uow;

namespace MultiTenantProductManagementApp;

[DependsOn(
    typeof(MultiTenantProductManagementAppApplicationModule),

    typeof(ProductServiceApplicationModule),
    typeof(StockServiceApplicationModule),

    typeof(ProductServiceEntityFrameworkCoreModule),
    typeof(StockServiceEntityFrameworkCoreModule)
)]
public class MultiTenantProductManagementAppApplicationTestModule : AbpModule
{
    private string? _connectionString;
    private static bool _dbInitialized;
    private static readonly object _initLock = new object();

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureSqlServerLocalDb(context.Services);
    }

    private void ConfigureSqlServerLocalDb(IServiceCollection services)
    {
        var resetEnv = Environment.GetEnvironmentVariable("RESET_TEST_DB");
        var resetDb = !string.IsNullOrWhiteSpace(resetEnv) && (resetEnv.Equals("1") || resetEnv.Equals("true", StringComparison.OrdinalIgnoreCase));

        var dbName = "MultiTenantProductManagementApp_Tests_MySql";
        _connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={dbName};Trusted_Connection=True;MultipleActiveResultSets=true";

services.Configure<AbpUnitOfWorkDefaultOptions>(opt =>
{
    opt.TransactionBehavior = UnitOfWorkTransactionBehavior.Disabled;
});
        services.Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(context =>
            {
                context.DbContextOptions.UseSqlServer(
                    _connectionString,
                    sql => sql.EnableRetryOnFailure()
                );
            });
        });

        services.Configure<AbpDbConnectionOptions>(opt =>
        {
            opt.ConnectionStrings.Default = _connectionString!;
            opt.ConnectionStrings["Default"] = _connectionString!;
        });

        var options = new DbContextOptionsBuilder<MultiTenantProductManagementApp.EntityFrameworkCore.MultiTenantProductManagementAppDbContext>()
            .UseSqlServer(_connectionString, sql => sql.EnableRetryOnFailure())
            .Options;

        var mutexName = "Global\\MultiTenantProductManagementApp_Tests_DB_Mutex";
        using var mutex = new Mutex(false, mutexName);
        mutex.WaitOne();
        try
        {
            if (!_dbInitialized)
            {
                if (resetDb)
                {
                    using var deleteDb = new MultiTenantProductManagementApp.EntityFrameworkCore.MultiTenantProductManagementAppDbContext(options);
                    deleteDb.Database.EnsureDeleted();
                }

                using (var db = new MultiTenantProductManagementApp.EntityFrameworkCore.MultiTenantProductManagementAppDbContext(options))
                {
                    try { db.Database.Migrate(); }
                    catch
                    {
                        // Only fall back to EnsureCreated when we're resetting the DB this run
                        if (resetDb)
                        {
                            db.Database.EnsureCreated();
                        }
                        // Otherwise swallow to avoid duplicate table creation in KEEP_TEST_DB runs
                    }
                }

                var productDbOptions = new DbContextOptionsBuilder<ProductServiceDbContext>()
                    .UseSqlServer(_connectionString, sql => sql.EnableRetryOnFailure())
                    .Options;
                using (var productDb = new ProductServiceDbContext(productDbOptions))
                {
                    try { productDb.Database.Migrate(); }
                    catch
                    {
                        // When multiple DbContexts share the same database, schema objects may already exist.
                        // Swallow migration errors here to avoid 'There is already an object named ...' during KEEP_TEST_DB runs.
                    }
                }

                var stockDbOptions = new DbContextOptionsBuilder<StockServiceDbContext>()
                    .UseSqlServer(_connectionString, sql => sql.EnableRetryOnFailure())
                    .Options;
                using (var stockDb = new StockServiceDbContext(stockDbOptions))
                {
                    try { stockDb.Database.Migrate(); }
                    catch
                    {
                        // See note above: avoid EnsureCreated() to prevent duplicate table creation; ignore if schema already exists.
                    }
                }

                lock (_initLock) { _dbInitialized = true; }
            }
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }
}