using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace MultiTenantProductManagementApp.Products;

public class ProductVariant : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid ProductId { get; protected set; }

    public string? Sku { get; protected set; }

    
    public string? Color { get; protected set; }
    
    public string? Size { get; protected set; } 

    public decimal Price { get; protected set; }

    public int StockQuantity { get; protected set; }

    protected ProductVariant() { }

    public ProductVariant(Guid id, Guid? tenantId, Guid productId, decimal price, int stockQuantity,
        string? sku = null, string? color = null, string? size = null)
        : base(id)
    {
        TenantId = tenantId;
        ProductId = productId;
        SetPrice(price);
        SetStock(stockQuantity);
        SetSku(sku);
        SetColor(color);
        SetSize(size);
    }

    public void SetSku(string? sku)
    {
        if (sku != null && sku.Length > 64)
            throw new ArgumentException("SKU max length is 64", nameof(sku));
        Sku = sku;
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

    public void SetColor(string? color)
    {
        if (color != null && color.Length > 50)
            throw new ArgumentException("Color max length is 50", nameof(color));
        Color = color;
    }

    public void SetSize(string? size)
    {
        if (size != null && size.Length > 20)
            throw new ArgumentException("Size max length is 20", nameof(size));
        Size = size;
    }
}
