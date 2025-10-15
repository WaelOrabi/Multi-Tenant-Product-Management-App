using MultiTenantProductManagementApp.Localization;
using MultiTenantProductManagementApp.Products.Settings;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace MultiTenantProductManagementApp.Products.Settings;

public class ProductSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                name: ProductSettings.AllowDuplicateNames,
                defaultValue: "false",
                displayName: L("Settings.Products.AllowDuplicateNames"),
                description: L("Settings.Products.AllowDuplicateNames.Description"),
                isVisibleToClients: true
            )
            .WithProviders(
                DefaultValueSettingValueProvider.ProviderName,
                TenantSettingValueProvider.ProviderName
            )
        );
    }
    
    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MultiTenantProductManagementAppResource>(name);
    }
}
