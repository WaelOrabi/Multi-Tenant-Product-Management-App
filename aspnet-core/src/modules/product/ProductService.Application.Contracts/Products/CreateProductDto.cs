using Volo.Abp.Application.Dtos;

namespace ProductService.InternalDtos;

// Internal-only placeholder to avoid name conflicts with legacy DTOs
public class ProductCreateRequestDto : IEntityDto
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal? BasePrice { get; set; }
    public string? Category { get; set; }
    public bool HasVariants { get; set; }
}
