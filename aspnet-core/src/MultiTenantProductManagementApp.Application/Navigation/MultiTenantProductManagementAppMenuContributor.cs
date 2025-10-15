using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MultiTenantProductManagementApp.Localization;
using Volo.Abp.Account.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.UI.Navigation;
using Volo.Abp.Users;
using Volo.Abp.SettingManagement.Localization;

namespace MultiTenantProductManagementApp;

public class MultiTenantProductManagementAppMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
        else if (context.Menu.Name == StandardMenus.User)
        {
            await ConfigureUserMenuAsync(context);
        }
    }

    private Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<MultiTenantProductManagementAppResource>();

        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                MultiTenantProductManagementAppMenus.Home,
                l["Menu:Home"],
                "~/",
                icon: "fas fa-home",
                order: 0
            )
        );

        // Add Administration menu
        var administration = context.Menu.GetAdministration();
        administration.Order = 100;

        return Task.CompletedTask;
    }

    private Task ConfigureUserMenuAsync(MenuConfigurationContext context)
    {
        var accountStringLocalizer = context.GetLocalizer<AccountResource>();
        var currentUser = context.ServiceProvider.GetRequiredService<ICurrentUser>();

        if (currentUser.IsAuthenticated)
        {
            context.Menu.AddItem(new ApplicationMenuItem(
                "Account.Manage",
                accountStringLocalizer["MyAccount"],
                url: "~/Account/Manage",
                icon: "fa fa-cog",
                order: 1000,
                target: "_blank"
            ));
        }

        return Task.CompletedTask;
    }
}