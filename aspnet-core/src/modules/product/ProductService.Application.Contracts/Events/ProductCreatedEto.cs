using System;

namespace ProductService.Events;

public class ProductCreatedEto
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = default!;
    public Guid? TenantId { get; set; }
}
