using Volo.Abp.Settings;

namespace MultiTenantProductManagementApp.Settings;

public class MultiTenantProductManagementAppSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(MultiTenantProductManagementAppSettings.MySetting1));
    }
}
