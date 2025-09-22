using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Guids;

namespace MultiTenantProductManagementApp.Shared;

public static class TestAdminSeeder
{
    public static async Task EnsureAdminAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetService<IdentityUserManager>();
        var guidGen = serviceProvider.GetService<IGuidGenerator>();
        if (userManager == null || guidGen == null)
        {
            return;
        }

        var adminUser = await userManager.FindByNameAsync("admin");
        if (adminUser == null)
        {
            adminUser = new IdentityUser(guidGen.Create(), "admin", "admin@tests.local");
            var createUserResult = await userManager.CreateAsync(adminUser, "1q2w3E*");
            if (!createUserResult.Succeeded)
            {
                var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                throw new Exception("Failed to create admin user: " + errors);
            }
        }
    }
}
