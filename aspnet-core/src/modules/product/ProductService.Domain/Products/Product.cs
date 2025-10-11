using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using Volo.Abp;
using MultiTenantProductManagementApp.Products; // for ProductStatus enum (shared)

namespace ProductService.Products;

public class Product : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string Name { get; protected set; } = default!;
    public string? Description { get; protected set; }

    public decimal? BasePrice { get; protected set; }

    public string? Category { get; protected set; }

    public ProductStatus Status { get; protected set; } = ProductStatus.Inactive;

    public bool HasVariants { get; protected set; }

    public ICollection<ProductVariant> Variants { get; protected set; } = new List<ProductVariant>();

    protected Product() { }

    public Product(Guid id, Guid? tenantId, string name, string? description = null, decimal? basePrice = null,
        string? category = null, ProductStatus status = ProductStatus.Inactive, bool hasVariants = false)
        : base(id)
    {
        TenantId = tenantId;
        SetName(name);
        Description = description;
        BasePrice = basePrice;
        Category = category;
        Status = status;
        HasVariants = hasVariants;
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), maxLength: 128);
    }

    public void SetDescription(string? description)
    {
        Description = description;
    }

    public void SetBasePrice(decimal? price)
    {
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
        BasePrice = price;
    }

    public void SetCategory(string? category)
    {
        if (category != null && category.Length > 64)
        {
            throw new ArgumentException("Category max length is 64", nameof(category));
        }
        Category = category;
    }

    public void SetStatus(ProductStatus status)
    {
        Status = status;
    }

    public void EnableVariants() => HasVariants = true;
    public void DisableVariants() => HasVariants = false;
}
