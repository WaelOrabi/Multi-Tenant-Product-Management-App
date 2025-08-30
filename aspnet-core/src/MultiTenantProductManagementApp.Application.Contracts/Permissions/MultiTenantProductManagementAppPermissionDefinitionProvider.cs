using MultiTenantProductManagementApp.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace MultiTenantProductManagementApp.Permissions;

public class MultiTenantProductManagementAppPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(MultiTenantProductManagementAppPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(MultiTenantProductManagementAppPermissions.MyPermission1, L("Permission:MyPermission1"));

        var products = myGroup.AddPermission(MultiTenantProductManagementAppPermissions.Products.Default, L("Permission:Products"));
        products.AddChild(MultiTenantProductManagementAppPermissions.Products.Create, L("Permission:Products.Create"));
        products.AddChild(MultiTenantProductManagementAppPermissions.Products.Edit, L("Permission:Products.Edit"));
        products.AddChild(MultiTenantProductManagementAppPermissions.Products.Delete, L("Permission:Products.Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MultiTenantProductManagementAppResource>(name);
    }
}
