using FluentValidation;
using MultiTenantProductManagementApp.Products.Dtos;
using Microsoft.Extensions.Localization;
using MultiTenantProductManagementApp.Localization;

namespace ProductService.Validation.Products;

public class CreateUpdateProductVariantDtoValidator : AbstractValidator<CreateUpdateProductVariantDto>
{
    private readonly IStringLocalizer<MultiTenantProductManagementAppResource> L;

    public CreateUpdateProductVariantDtoValidator(IStringLocalizer<MultiTenantProductManagementAppResource> localizer)
    {
        L = localizer;
        RuleFor(x => x.Sku)
            .MaximumLength(64)
            .WithMessage(L["Product.Validation.SkuMaxLength", 64]);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage(L["Product.Validation.VariantPriceNonNegative"]);

        When(x => x.Options != null, () =>
        {
            RuleForEach(x => x.Options!)
                .SetValidator(new ProductVariantOptionDtoValidator(L));
        });
    }
}
