using System;
using Volo.Abp.Application.Dtos;
using System.Collections.Generic;

namespace MultiTenantProductManagementApp.Products.Dtos;

public class ProductVariantDto : AuditedEntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public List<ProductVariantOptionDto>? Options { get; set; }
}
