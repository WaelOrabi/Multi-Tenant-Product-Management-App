using System;
using Volo.Abp.Application.Dtos;

namespace ProductService.InternalDtos;

public class ProductViewDto : EntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal? BasePrice { get; set; }
    public string? Category { get; set; }
    public bool HasVariants { get; set; }
}
