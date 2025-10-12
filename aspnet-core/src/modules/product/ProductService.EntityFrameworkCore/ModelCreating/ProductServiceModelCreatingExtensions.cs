using Microsoft.EntityFrameworkCore;
using ProductService.Products;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ProductService;

public static class ProductServiceModelCreatingExtensions
{
    public static void ConfigureProductService(this ModelBuilder builder)
    {
        builder.Entity<Product>(b =>
        {
            b.ToTable("Products");
            b.ConfigureByConvention(); 
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Category).HasMaxLength(64);
            b.Property(x => x.BasePrice).HasPrecision(18, 2);
            b.HasMany(x => x.Variants).WithOne().HasForeignKey(v => v.ProductId);
        });

        builder.Entity<ProductVariant>(b =>
        {
            b.ToTable("ProductVariants");
            b.ConfigureByConvention();
            b.Property(x => x.Sku).HasMaxLength(64);
            b.Property(x => x.Price).HasPrecision(18, 2);

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
