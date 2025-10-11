using System;
using System.Collections.Generic;
using MultiTenantProductManagementApp.Products;
using Volo.Abp.Application.Dtos;

namespace MultiTenantProductManagementApp.Products.Dtos;

public class ProductDto : AuditedEntityDto<Guid>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal? BasePrice { get; set; }
    public string? Category { get; set; }
    public ProductStatus Status { get; set; }
    public bool HasVariants { get; set; }

    public List<ProductVariantDto> Variants { get; set; } = new();
}
