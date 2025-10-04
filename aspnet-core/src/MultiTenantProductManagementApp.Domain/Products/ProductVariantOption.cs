using System;

namespace MultiTenantProductManagementApp.Products;

public class ProductVariantOption
{
    public string Name { get; protected set; } = default!;
    public string Value { get; protected set; } = default!;

    protected ProductVariantOption() { }

    public ProductVariantOption(string name, string value)
    {
        SetName(name);
        SetValue(value);
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Option name is required", nameof(name));
    
        Name = name.Trim();
    }

    public void SetValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Option value is required", nameof(value));
        
        Value = value.Trim();
    }
}
