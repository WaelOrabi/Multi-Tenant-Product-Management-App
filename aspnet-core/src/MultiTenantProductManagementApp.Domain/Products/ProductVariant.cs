using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace MultiTenantProductManagementApp.Products;

public class ProductVariant : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid ProductId { get; protected set; }

    public string? Sku { get; protected set; }

    public string? AttributesJson { get; protected set; } 

    public decimal Price { get; protected set; }

    public int StockQuantity { get; protected set; }

    protected ProductVariant() { }

    public ProductVariant(Guid id, Guid? tenantId, Guid productId, decimal price, int stockQuantity,
        string? sku = null, string? attributesJson = null)
        : base(id)
    {
        TenantId = tenantId;
        ProductId = productId;
        SetPrice(price);
        SetStock(stockQuantity);
        SetSku(sku);
        SetAttributes(attributesJson);
    }

    public void SetSku(string? sku)
    {
        if (sku != null && sku.Length > 64)
            throw new ArgumentException("SKU max length is 64", nameof(sku));
        Sku = sku;
    }

    public void SetAttributes(string? json)
    {
        AttributesJson = json;
    }

    public void SetPrice(decimal price)
    {
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
        Price = price;
    }

    public void SetStock(int qty)
    {
        if (qty < 0) throw new ArgumentOutOfRangeException(nameof(qty));
        StockQuantity = qty;
    }
}
