using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MultiTenantProductManagementApp.Products;

namespace MultiTenantProductManagementApp.Products.Dtos;

public class CreateUpdateProductDto
{
    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = default!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    public decimal? BasePrice { get; set; }

    [MaxLength(64)]
    public string? Category { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    public bool HasVariants { get; set; }

    public List<CreateUpdateProductVariantDto> Variants { get; set; } = new();
}
