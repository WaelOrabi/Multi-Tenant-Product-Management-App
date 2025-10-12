using System.Linq;
using FluentValidation;
using MultiTenantProductManagementApp.Products.Dtos;
using Microsoft.Extensions.Localization;
using MultiTenantProductManagementApp.Localization;

namespace ProductService.Validation.Products;

public class CreateUpdateProductDtoValidator : AbstractValidator<CreateUpdateProductDto>
{
    private readonly IStringLocalizer<MultiTenantProductManagementAppResource> L;

    public CreateUpdateProductDtoValidator(IStringLocalizer<MultiTenantProductManagementAppResource> localizer)
    {
        L = localizer;
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Description)
            .MaximumLength(1024);

        RuleFor(x => x.Category)
            .MaximumLength(64);

        When(x => !x.HasVariants, () =>
        {
            RuleFor(x => x.BasePrice)
                .NotNull()
                .WithMessage(L["Product.Validation.BasePriceRequired"])
                .GreaterThanOrEqualTo(0)
                .WithMessage(L["Product.Validation.BasePriceNonNegative"]);

            RuleFor(x => x.Variants)
                .Must(v => v == null || v.Count == 0)
                .WithMessage(L["Product.Validation.VariantsEmptyWhenNoVariants"]);
        });

        When(x => x.HasVariants, () =>
        {
            RuleFor(x => x.Variants)
                .NotNull()
                .WithMessage(L["Product.Validation.VariantsRequired"]) 
                .Must(v => v!.Count > 0)
                .WithMessage(L["Product.Validation.AtLeastOneVariant"]);

            RuleForEach(x => x.Variants)
                .SetValidator(new CreateUpdateProductVariantDtoValidator());

            RuleFor(x => x)
                .Must(x =>
                {
                    var skus = (x.Variants ?? []).Select(v => v.Sku).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!.Trim()).ToList();
                    return skus.Count == skus.Distinct().Count();
                })
                .WithMessage(L["Product.Validation.VariantSkusUnique"]);
        });

        RuleFor(x => x.BasePrice)
            .Must(p => p == null || p >= 0)
            .WithMessage(L["Product.Validation.BasePriceNonNegative"]);
    }
}
