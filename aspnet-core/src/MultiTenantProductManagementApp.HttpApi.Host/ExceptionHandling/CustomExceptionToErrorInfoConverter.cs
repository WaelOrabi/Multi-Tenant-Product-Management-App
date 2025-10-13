using System;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.ExceptionHandling.Localization;
using Volo.Abp.Http;

namespace MultiTenantProductManagementApp.ExceptionHandling
{
    public class CustomExceptionToErrorInfoConverter : IExceptionToErrorInfoConverter
    {
        private readonly DefaultExceptionToErrorInfoConverter _inner;

        public CustomExceptionToErrorInfoConverter(
            IOptions<AbpExceptionLocalizationOptions> localizationOptions,
            IStringLocalizerFactory stringLocalizerFactory,
            IStringLocalizer<Volo.Abp.ExceptionHandling.Localization.AbpExceptionHandlingResource> abpExceptionHandlingLocalizer,
            IServiceProvider serviceProvider)
        {
            _inner = new DefaultExceptionToErrorInfoConverter(
                localizationOptions,
                stringLocalizerFactory,
                abpExceptionHandlingLocalizer,
                serviceProvider
            );
        }

        public RemoteServiceErrorInfo Convert(Exception exception, bool includeSensitiveDetails)
        {
            var info = _inner.Convert(exception, includeSensitiveDetails);

            // Preserve BusinessException.Code
            if (exception is BusinessException be && !string.IsNullOrWhiteSpace(be.Code))
            {
                info.Code = be.Code;
            }

            // Allow other exceptions (e.g., AbpValidationException) to set a specific code via exception.Data["Code"]
            if (exception.Data != null && exception.Data.Contains("Code"))
            {
                info.Code = exception.Data["Code"]?.ToString();
            }

            // Normalize validation error message text
            if (exception is Volo.Abp.Validation.AbpValidationException)
            {
                info.Message = "Your request is not valid, please correct and try again!";
            }

            return info;
        }

        public RemoteServiceErrorInfo Convert(Exception exception, Action<AbpExceptionHandlingOptions>? optionsAction)
        {
            var info = _inner.Convert(exception, optionsAction);

            if (exception is BusinessException be && !string.IsNullOrWhiteSpace(be.Code))
            {
                info.Code = be.Code;
            }

            if (exception.Data != null && exception.Data.Contains("Code"))
            {
                info.Code = exception.Data["Code"]?.ToString();
            }

            if (exception is Volo.Abp.Validation.AbpValidationException)
            {
                info.Message = "Your request is not valid, please correct and try again!";
            }

            return info;
        }
    }
}
