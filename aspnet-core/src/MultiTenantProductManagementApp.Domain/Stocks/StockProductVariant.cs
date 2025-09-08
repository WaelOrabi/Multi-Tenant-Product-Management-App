using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace MultiTenantProductManagementApp.Stocks;

public class StockProductVariant : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid StockProductId { get; protected set; }

    public Guid? ProductVariantId { get; protected set; }

    public int Quantity { get; protected set; }

    protected StockProductVariant() { }

    public StockProductVariant(Guid id, Guid? tenantId, Guid stockProductId, Guid? productVariantId, int quantity) : base(id)
    {
        TenantId = tenantId;
        StockProductId = stockProductId;
        ProductVariantId = productVariantId;
        SetQuantity(quantity);
    }

    public void SetQuantity(int quantity)
    {
        if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        Quantity = quantity;
    }
}
