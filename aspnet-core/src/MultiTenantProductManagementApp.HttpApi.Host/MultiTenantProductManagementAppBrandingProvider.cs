using Microsoft.Extensions.Localization;
using MultiTenantProductManagementApp.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace MultiTenantProductManagementApp;

[Dependency(ReplaceServices = true)]
public class MultiTenantProductManagementAppBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<MultiTenantProductManagementAppResource> _localizer;

    public MultiTenantProductManagementAppBrandingProvider(IStringLocalizer<MultiTenantProductManagementAppResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
