using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MultiTenantProductManagementApp.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class MultiTenantProductManagementAppDbContextFactory : IDesignTimeDbContextFactory<MultiTenantProductManagementAppDbContext>
{
    public MultiTenantProductManagementAppDbContext CreateDbContext(string[] args)
    {
        MultiTenantProductManagementAppEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<MultiTenantProductManagementAppDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));

        return new MultiTenantProductManagementAppDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../MultiTenantProductManagementApp.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
