using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace MultiTenantProductManagementApp.Stocks;

public class Stock : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(128)]
    public string Name { get; protected set; } = default!;

    public ICollection<StockProduct> Products { get; protected set; } = new List<StockProduct>();

    protected Stock() { }

    public Stock(Guid id, Guid? tenantId, string name)
        : base(id)
    {
        TenantId = tenantId;
        SetName(name);
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));
        if (name.Length > 128) throw new ArgumentException("Name max length is 128", nameof(name));
        Name = name.Trim();
    }
}
