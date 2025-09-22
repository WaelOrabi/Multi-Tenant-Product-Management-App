using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore;
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
using MultiTenantProductManagementApp.Shared;

namespace MultiTenantProductManagementApp.EntityFrameworkCore;

[DependsOn(
    typeof(MultiTenantProductManagementAppApplicationTestModule),
    typeof(MultiTenantProductManagementAppEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule)
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
                using (var db = new MultiTenantProductManagementAppDbContext(options))
                {
                    if (resetDb)
                    {
                        Console.WriteLine("[EFTest] RESET_TEST_DB is set. Recreating test database...");
                        db.Database.EnsureDeleted();
                    }
                db.Database.EnsureCreated();
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
                await TestAdminSeeder.EnsureAdminAsync(scope.ServiceProvider);
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
