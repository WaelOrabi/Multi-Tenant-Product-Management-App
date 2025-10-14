using MultiTenantProductManagementApp.Localization;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Features;
using Volo.Abp.Localization;

namespace MultiTenantProductManagementApp.Features;

public class MultiTenantProductManagementAppFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(MultiTenantProductManagementAppFeatures.GroupName, L("Feature:GroupName"));

        group.AddFeature(
            MultiTenantProductManagementAppFeatures.Product.Variants,
            defaultValue: "false",
            displayName: L("Feature:Variants"),
            description: L("Feature:Variants.Description")
        );

        group.AddFeature(
            MultiTenantProductManagementAppFeatures.Inventory.Stock,
            defaultValue: "false",
            displayName: L("Feature:Stock"),
            description: L("Feature:Stock.Description")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MultiTenantProductManagementAppResource>(name);
    }
}
