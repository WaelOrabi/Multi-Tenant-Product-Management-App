using Microsoft.EntityFrameworkCore;
using ProductService.Products;

namespace ProductService.Extensions;

public static class ProductServiceModelBuilderExtensions
{
    public static void ConfigureProductService(this ModelBuilder builder)
    {
        // Product
        builder.Entity<Product>(b =>
        {
            b.ToTable("Products");
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Category).HasMaxLength(64);
            // precision for decimal
            b.Property(x => x.BasePrice).HasPrecision(18, 2);
            b.HasMany(x => x.Variants).WithOne().HasForeignKey(v => v.ProductId);
        });

        // ProductVariant
        builder.Entity<ProductVariant>(b =>
        {
            b.ToTable("ProductVariants");
            b.Property(x => x.Sku).HasMaxLength(64);
            b.Property(x => x.Price).HasPrecision(18, 2);

            // Owned options as separate table
            b.OwnsMany(v => v.Options, o =>
            {
                o.WithOwner().HasForeignKey("ProductVariantId");
                o.ToTable("ProductVariantOptions");
                o.Property<int>("Id").ValueGeneratedOnAdd();
                o.HasKey("Id");
                o.Property(x => x.Name).IsRequired().HasMaxLength(64);
                o.Property(x => x.Value).IsRequired().HasMaxLength(128);
                o.HasIndex("ProductVariantId", "Name");
            });
        });
    }
}
