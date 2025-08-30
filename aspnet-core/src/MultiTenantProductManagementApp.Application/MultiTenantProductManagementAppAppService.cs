using System;
using System.Collections.Generic;
using System.Text;
using MultiTenantProductManagementApp.Localization;
using Volo.Abp.Application.Services;

namespace MultiTenantProductManagementApp;

/* Inherit your application services from this class.
 */
public abstract class MultiTenantProductManagementAppAppService : ApplicationService
{
    protected MultiTenantProductManagementAppAppService()
    {
        LocalizationResource = typeof(MultiTenantProductManagementAppResource);
    }
}
