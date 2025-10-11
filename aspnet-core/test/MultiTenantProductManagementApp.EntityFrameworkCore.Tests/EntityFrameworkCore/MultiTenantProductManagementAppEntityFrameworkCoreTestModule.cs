using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Uow;
using Volo.Abp.Autofac;
using Volo.Abp.Castle;


namespace MultiTenantProductManagementApp.EntityFrameworkCore;

[DependsOn(
    typeof(MultiTenantProductManagementAppEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpAutofacModule),
    typeof(AbpCastleCoreModule) 

    )]
public class MultiTenantProductManagementAppEntityFrameworkCoreTestModule : AbpModule
{
    private string? _connectionString;
    private static bool _dbInitialized;
    private static bool _adminSeeded;
    private static readonly object _initLock = new object();

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<FeatureManagementOptions>(options =>
        {
            options.SaveStaticFeaturesToDatabase = false;
            options.IsDynamicFeatureStoreEnabled = false;
        });
        Configure<PermissionManagementOptions>(options =>
        {
            options.SaveStaticPermissionsToDatabase = false;
            options.IsDynamicPermissionStoreEnabled = false;
        });
        Configure<SettingManagementOptions>(options =>
        {
            options.SaveStaticSettingsToDatabase = false;
            options.IsDynamicSettingStoreEnabled = false;
        });
        context.Services.AddAlwaysDisableUnitOfWorkTransaction();

        ConfigureSqlServerLocalDb(context.Services);
    }

    private void ConfigureSqlServerLocalDb(IServiceCollection services)
    {
        var resetEnv = Environment.GetEnvironmentVariable("RESET_TEST_DB");
        var resetDb = !string.IsNullOrWhiteSpace(resetEnv) && (resetEnv.Equals("1") || resetEnv.Equals("true", StringComparison.OrdinalIgnoreCase));

        var dbName = "MultiTenantProductManagementApp_Tests_MySql"; 
        _connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={dbName};Trusted_Connection=True;MultipleActiveResultSets=true";

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

        var options = new DbContextOptionsBuilder<MultiTenantProductManagementAppDbContext>()
            .UseSqlServer(_connectionString, sql => sql.EnableRetryOnFailure())
            .Options;

        var mutexName = "Global\\MultiTenantProductManagementApp_Tests_DB_Mutex";
        using var mutex = new Mutex(false, mutexName);
        mutex.WaitOne();
        try
        {
            if (!_dbInitialized)
            {
                // If resetting, delete the database ONCE using the main app DbContext
                if (resetDb)
                {
                    Console.WriteLine("[EFTest] RESET_TEST_DB is set. Recreating test database...");
                    using (var deleteDb = new MultiTenantProductManagementAppDbContext(options))
                    {
                        deleteDb.Database.EnsureDeleted();
                    }
                }

                // Create main app database (includes Identity/Abp tables)
                using (var db = new MultiTenantProductManagementAppDbContext(options))
                {
                    try
                    {
                        db.Database.Migrate();
                    }
                    catch
                    {
                        // Fallback for environments without migrations
                        db.Database.EnsureCreated();
                    }
                }

                // Also ensure module databases exist (no deletions here, same database/connection)
                var productDbOptions = new DbContextOptionsBuilder<ProductService.ProductServiceDbContext>()
                    .UseSqlServer(_connectionString, sql => sql.EnableRetryOnFailure())
                    .Options;
                using (var productDb = new ProductService.ProductServiceDbContext(productDbOptions))
                {
                    // Apply migrations if available (no-op when there are none)
                    productDb.Database.Migrate();
                    // Ensure tables exist even when there are zero migrations
                    var ensured = productDb.Database.EnsureCreated();
                    if (!ensured)
                    {
                        var creator = productDb.Database.GetService<IRelationalDatabaseCreator>();
                        creator.CreateTables();
                    }
                }
                
                var stockDbOptions = new DbContextOptionsBuilder<StockService.StockServiceDbContext>()
                    .UseSqlServer(_connectionString, sql => sql.EnableRetryOnFailure())
                    .Options;
                using (var stockDb = new StockService.StockServiceDbContext(stockDbOptions))
                {
                    // Apply migrations if available (no-op when there are none)
                    stockDb.Database.Migrate();
                    // Ensure tables exist even when there are zero migrations
                    var ensuredStock = stockDb.Database.EnsureCreated();
                    if (!ensuredStock)
                    {
                        var creator = stockDb.Database.GetService<IRelationalDatabaseCreator>();
                        creator.CreateTables();
                    }
                }
                
                lock (_initLock)
                {
                    _dbInitialized = true;
                }
            }
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        using var scope = context.ServiceProvider.CreateScope();
        if (!_adminSeeded)
        {
            Task.Run(async () =>
            {
                var userRepo = scope.ServiceProvider.GetRequiredService<IIdentityUserRepository>();
                var userManager = scope.ServiceProvider.GetRequiredService<IdentityUserManager>();
                var guidGen = scope.ServiceProvider.GetRequiredService<Volo.Abp.Guids.IGuidGenerator>();

                var existing = await userRepo.FindByNormalizedUserNameAsync("ADMIN");
                if (existing == null)
                {
                    var user = new IdentityUser(guidGen.Create(), "admin", "admin@local");
                    await userManager.CreateAsync(user, "1q2w3E*");
                }
            }).GetAwaiter().GetResult();
            lock (_initLock)
            {
                _adminSeeded = true;
            }
        }
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
    }
}
