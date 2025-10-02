using System;
using Volo.Abp.Application.Dtos;

namespace MultiTenantProductManagementApp.Products.Dtos;

public class ProductVariantDto : AuditedEntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string? Sku { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal Price { get; set; }
}
