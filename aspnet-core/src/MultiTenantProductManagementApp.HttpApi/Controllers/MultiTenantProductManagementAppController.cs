using MultiTenantProductManagementApp.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace MultiTenantProductManagementApp.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class MultiTenantProductManagementAppController : AbpControllerBase
{
    protected MultiTenantProductManagementAppController()
    {
        LocalizationResource = typeof(MultiTenantProductManagementAppResource);
    }
}
