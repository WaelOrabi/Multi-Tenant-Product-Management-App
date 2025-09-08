using MultiTenantProductManagementApp.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace MultiTenantProductManagementApp.Permissions;

public class MultiTenantProductManagementAppPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(MultiTenantProductManagementAppPermissions.GroupName);

        var products = myGroup.AddPermission(MultiTenantProductManagementAppPermissions.Products.Default, L("Permission:Products"));
        products.AddChild(MultiTenantProductManagementAppPermissions.Products.Create, L("Permission:Products.Create"));
        products.AddChild(MultiTenantProductManagementAppPermissions.Products.Edit, L("Permission:Products.Edit"));
        products.AddChild(MultiTenantProductManagementAppPermissions.Products.Delete, L("Permission:Products.Delete"));

        var stocks = myGroup.AddPermission(MultiTenantProductManagementAppPermissions.Stocks.Default, L("Permission:Stocks"));
        stocks.AddChild(MultiTenantProductManagementAppPermissions.Stocks.Create, L("Permission:Stocks.Create"));
        stocks.AddChild(MultiTenantProductManagementAppPermissions.Stocks.Edit, L("Permission:Stocks.Edit"));
        stocks.AddChild(MultiTenantProductManagementAppPermissions.Stocks.Delete, L("Permission:Stocks.Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MultiTenantProductManagementAppResource>(name);
    }
}
