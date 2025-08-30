using System;
using System.ComponentModel.DataAnnotations;

namespace MultiTenantProductManagementApp.Products.Dtos;

public class CreateUpdateProductVariantDto
{
    [MaxLength(64)]
    public string? Sku { get; set; }

    public string? AttributesJson { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}
