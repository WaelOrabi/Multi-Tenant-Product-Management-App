using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MultiTenantProductManagementApp.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Guids;
using Volo.Abp.PermissionManagement;

namespace MultiTenantProductManagementApp.Permissions;

public class AdminRolePermissionDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IdentityRoleManager _roleManager;
    private readonly ILogger<AdminRolePermissionDataSeedContributor> _logger;
    private readonly ITenantRepository _tenantRepository;
    private readonly IGuidGenerator _guidGenerator;

    public AdminRolePermissionDataSeedContributor(
        ICurrentTenant currentTenant,
        IPermissionDataSeeder permissionDataSeeder,
        IdentityRoleManager roleManager,
        ILogger<AdminRolePermissionDataSeedContributor> logger,
        ITenantRepository tenantRepository,
        IGuidGenerator guidGenerator)
    {
        _currentTenant = currentTenant;
        _permissionDataSeeder = permissionDataSeeder;
        _roleManager = roleManager;
        _logger = logger;
        _tenantRepository = tenantRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // Seed for each tenant
        var tenants = await _tenantRepository.GetListAsync();
        foreach (var tenant in tenants)
        {
            using (_currentTenant.Change(tenant.Id))
            {
                // Ensure admin role exists
                var role = await _roleManager.FindByNameAsync("admin");
                if (role == null)
                {
                    role = new IdentityRole(_guidGenerator.Create(), "admin", tenant.Id);
                    var createResult = await _roleManager.CreateAsync(role);
                    if (!createResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to create admin role for tenant {Tenant}: {Errors}", tenant.Name, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                        continue;
                    }
                }

                var permissions = new[]
                {
                    // Products
                    MultiTenantProductManagementAppPermissions.Products.Default,
                    MultiTenantProductManagementAppPermissions.Products.Create,
                    MultiTenantProductManagementAppPermissions.Products.Edit,
                    MultiTenantProductManagementAppPermissions.Products.Delete,

                    // Identity - Users
                    IdentityPermissions.Users.Default,
                    IdentityPermissions.Users.Create,
                    IdentityPermissions.Users.Update,
                    IdentityPermissions.Users.Delete,
                    IdentityPermissions.Users.ManagePermissions,

                    // Identity - Roles
                    IdentityPermissions.Roles.Default,
                    IdentityPermissions.Roles.Create,
                    IdentityPermissions.Roles.Update,
                    IdentityPermissions.Roles.Delete,
                    IdentityPermissions.Roles.ManagePermissions
                };

                await _permissionDataSeeder.SeedAsync(
                    RolePermissionValueProvider.ProviderName,
                    "admin",
                    permissions,
                    tenant.Id
                );
            }
        }
    }
}
