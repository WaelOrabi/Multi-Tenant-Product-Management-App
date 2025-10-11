using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductService.Products;

namespace ProductService.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.ToTable("Products");
        b.Property(x => x.Name).IsRequired().HasMaxLength(128);
        b.Property(x => x.Category).HasMaxLength(64);
        b.Property(x => x.BasePrice).HasPrecision(18, 2);
        b.HasMany(x => x.Variants).WithOne().HasForeignKey(v => v.ProductId);
    }
}
