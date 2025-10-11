using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductService.Products;

namespace ProductService.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> b)
    {
        b.ToTable("ProductVariants");
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
    }
}
