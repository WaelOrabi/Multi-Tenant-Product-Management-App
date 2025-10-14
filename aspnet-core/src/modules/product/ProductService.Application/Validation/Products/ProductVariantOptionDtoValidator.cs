using FluentValidation;
using MultiTenantProductManagementApp.Products.Dtos;
using Microsoft.Extensions.Localization;
using MultiTenantProductManagementApp.Localization;

namespace ProductService.Validation.Products;

public class ProductVariantOptionDtoValidator : AbstractValidator<ProductVariantOptionDto>
{
    private readonly IStringLocalizer<MultiTenantProductManagementAppResource> L;

    public ProductVariantOptionDtoValidator(IStringLocalizer<MultiTenantProductManagementAppResource> localizer)
    {
        L = localizer;
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(L["Product.Validation.OptionNameRequired"]) 
            .MaximumLength(64).WithMessage(L["Product.Validation.OptionNameMaxLength", 64]);

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage(L["Product.Validation.OptionValueRequired"]) 
            .MaximumLength(128).WithMessage(L["Product.Validation.OptionValueMaxLength", 128]);
    }
}
