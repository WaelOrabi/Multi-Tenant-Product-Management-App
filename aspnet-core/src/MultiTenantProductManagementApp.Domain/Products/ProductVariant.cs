using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace MultiTenantProductManagementApp.Products;

public class ProductVariant : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid ProductId { get; protected set; }

    public string? Sku { get; protected set; }

    public decimal Price { get; protected set; }

    public List<ProductVariantOption> Options { get; private set; } = new();

    protected ProductVariant() { }

    public ProductVariant(Guid id, Guid? tenantId, Guid productId, decimal price,
        string? sku = null, IEnumerable<ProductVariantOption>? options = null)
        : base(id)
    {
        TenantId = tenantId;
        ProductId = productId;
        SetPrice(price);
        SetSku(sku);
        if (options != null)
        {
            ReplaceOptions(options);
        }
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

    public void AddOrUpdateOption(string name, string value)
    {
        var existingIndex = Options.FindIndex(o => string.Equals(o.Name, name, StringComparison.OrdinalIgnoreCase));
        if (existingIndex >= 0)
        {
            Options[existingIndex].SetValue(value);
        }
        else
        {
            Options.Add(new ProductVariantOption(name, value));
        }
    }

    public void RemoveOption(string name)
    {
        Options.RemoveAll(o => string.Equals(o.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public void ReplaceOptions(IEnumerable<ProductVariantOption> options)
    {
        Options.Clear();
        foreach (var o in options)
        {
            // enforce validation through setters
            var item = new ProductVariantOption(o.Name, o.Value);
            Options.Add(item);
        }
    }
}
