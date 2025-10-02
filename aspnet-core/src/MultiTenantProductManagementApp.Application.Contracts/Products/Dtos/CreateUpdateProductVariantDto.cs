using System;
using System.ComponentModel.DataAnnotations;

namespace MultiTenantProductManagementApp.Products.Dtos;

public class CreateUpdateProductVariantDto
{
    [MaxLength(64)]
    public string? Sku { get; set; }


    [MaxLength(50)]
    public string? Color { get; set; }

    [MaxLength(20)]
    public string? Size { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}
