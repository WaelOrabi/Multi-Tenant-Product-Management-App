using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MultiTenantProductManagementApp.Products.Dtos;

public class CreateUpdateProductVariantDto
{
    [MaxLength(64)]
    public string? Sku { get; set; }

    public List<ProductVariantOptionDto>? Options { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}
