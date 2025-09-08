using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace MultiTenantProductManagementApp.Stocks;

public class StockProduct : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid StockId { get; protected set; }

    public Guid ProductId { get; protected set; }

    public ICollection<StockProductVariant> Variants { get; protected set; } = new List<StockProductVariant>();

    protected StockProduct() { }

    public StockProduct(Guid id, Guid? tenantId, Guid stockId, Guid productId) : base(id)
    {
        TenantId = tenantId;
        StockId = stockId;
        ProductId = productId;
    }
}
