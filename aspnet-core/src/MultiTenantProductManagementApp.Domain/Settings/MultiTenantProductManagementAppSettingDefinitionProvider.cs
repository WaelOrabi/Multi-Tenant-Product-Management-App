using MultiTenantProductManagementApp.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace MultiTenantProductManagementApp.Settings;

public class MultiTenantProductManagementAppSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                name: MultiTenantProductManagementAppSettings.Products.AllowDuplicateNames,
                defaultValue: "false",
                displayName: L("Settings.Products.AllowDuplicateNames"),
                description: L("c"),
                isVisibleToClients: true
            ).WithProviders(TenantSettingValueProvider.ProviderName)
        );
        
    }
        private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MultiTenantProductManagementAppResource>(name);
    }
}
