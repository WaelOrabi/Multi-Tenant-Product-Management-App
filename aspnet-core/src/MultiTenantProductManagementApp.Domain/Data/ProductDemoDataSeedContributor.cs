using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using MultiTenantProductManagementApp.Products;
using Volo.Abp.Guids;
using Volo.Abp.PermissionManagement;

namespace MultiTenantProductManagementApp.Data;

public class ProductDemoDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly ITenantRepository _tenantRepository;
    private readonly TenantManager _tenantManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly IdentityUserManager _userManager;
    private readonly IdentityRoleManager _roleManager;
    private readonly IRepository<Product, Guid> _productRepo;
    private readonly IRepository<ProductVariant, Guid> _variantRepo;
    private readonly ILogger<ProductDemoDataSeedContributor> _logger;
    private readonly IGuidGenerator _guidGenerator;
   
    public ProductDemoDataSeedContributor(
        ITenantRepository tenantRepository,
        TenantManager tenantManager,
        ICurrentTenant currentTenant,
        IdentityUserManager userManager,
        IdentityRoleManager roleManager,
        IRepository<Product, Guid> productRepo,
        IRepository<ProductVariant, Guid> variantRepo,
        ILogger<ProductDemoDataSeedContributor> logger,
        IGuidGenerator guidGenerator,
        IPermissionDataSeeder permissionDataSeeder)
    {
        _tenantRepository = tenantRepository;
        _tenantManager = tenantManager;
        _currentTenant = currentTenant;
        _userManager = userManager;
        _roleManager = roleManager;
        _productRepo = productRepo;
        _variantRepo = variantRepo;
        _logger = logger;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var tenant1 = await EnsureTenantAsync("store-one");
        var tenant2 = await EnsureTenantAsync("store-two");

        await SeedTenantDataAsync(tenant1, "admin1@demo.com");
        await SeedTenantDataAsync(tenant2, "admin2@demo.com");
    }

    private async Task<Tenant> EnsureTenantAsync(string name)
    {
        var tenant = await _tenantRepository.FindByNameAsync(name);
        if (tenant != null)
        {
            return tenant;
        }
        tenant = await _tenantManager.CreateAsync(name);
        await _tenantRepository.InsertAsync(tenant, autoSave: true);
        _logger.LogInformation("Created tenant {TenantName}", name);
        return tenant;
    }

    private async Task SeedTenantDataAsync(Tenant tenant, string adminEmail)
    {
        using (_currentTenant.Change(tenant.Id))
        {
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser(_guidGenerator.Create(), adminEmail, adminEmail, tenant.Id);
                var createResult = await _userManager.CreateAsync(adminUser, "1q2w3E*");
                if (!createResult.Succeeded)
                {
                    _logger.LogWarning("Failed to create admin user for {Tenant}: {Errors}", tenant.Name, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }
    

            var adminRole = await _roleManager.FindByNameAsync("admin");
            if (adminRole == null)
            {
                adminRole = new IdentityRole(_guidGenerator.Create(), "admin", tenant.Id);
                var roleCreateResult = await _roleManager.CreateAsync(adminRole);
                if (!roleCreateResult.Succeeded)
                {
                    _logger.LogWarning("Failed to create admin role for {Tenant}: {Errors}", tenant.Name, string.Join(", ", roleCreateResult.Errors.Select(e => e.Description)));
                }
            }

            if (!await _userManager.IsInRoleAsync(adminUser, "admin"))
            {
                var addRoleResult = await _userManager.AddToRoleAsync(adminUser, "admin");
                if (!addRoleResult.Succeeded)
                {
                    _logger.LogWarning("Failed to add user {User} to admin role for {Tenant}: {Errors}", adminEmail, tenant.Name, string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
                }
            }

            if (await _productRepo.AnyAsync())
            {
                return;
            }

            var p1 = new Product(_guidGenerator.Create(), tenant.Id, "Basic T-Shirt", "Soft cotton tee", 19.99m, "Apparel", ProductStatus.Active, hasVariants: false);
            await _productRepo.InsertAsync(p1, autoSave: true);

            var p2 = new Product(_guidGenerator.Create(), tenant.Id, "Sneakers Pro", "Lightweight running shoes", null, "Footwear", ProductStatus.Active, hasVariants: true);
            await _productRepo.InsertAsync(p2, autoSave: true);

            var v1 = new ProductVariant(_guidGenerator.Create(), tenant.Id, p2.Id, 79.99m,  "SNK-BLK-42");
            var v2 = new ProductVariant(_guidGenerator.Create(), tenant.Id, p2.Id, 79.99m,  "SNK-BLK-43");
            var v3 = new ProductVariant(_guidGenerator.Create(), tenant.Id, p2.Id, 84.99m,  "SNK-RED-42");

            await _variantRepo.InsertManyAsync(new[] { v1, v2, v3 }, autoSave: true);
        }
    }
}
