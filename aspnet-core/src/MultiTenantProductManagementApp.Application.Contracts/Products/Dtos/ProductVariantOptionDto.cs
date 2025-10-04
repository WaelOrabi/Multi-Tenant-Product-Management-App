using System.ComponentModel.DataAnnotations;

namespace MultiTenantProductManagementApp.Products.Dtos;

public class ProductVariantOptionDto
{
    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = default!;

    [Required]
    [MaxLength(128)]
    public string Value { get; set; } = default!;
}
