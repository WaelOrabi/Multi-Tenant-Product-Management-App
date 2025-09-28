using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace MultiTenantProductManagementApp.Products.EntityFrameworkCore;

public static class ProductsDbContextModelCreatingExtensions
{
    public static void ConfigureProducts(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Product>(b =>
        {
            b.ToTable(MultiTenantProductManagementAppConsts.DbTablePrefix + "Products", MultiTenantProductManagementAppConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Category).HasMaxLength(64);
            b.Property(x => x.BasePrice).HasColumnType("decimal(18,2)");
            b.HasMany(x => x.Variants).WithOne().HasForeignKey(v => v.ProductId).IsRequired();
            b.HasIndex(x => new { x.TenantId, x.Name })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
        });

        builder.Entity<ProductVariant>(b =>
        {
            b.ToTable(MultiTenantProductManagementAppConsts.DbTablePrefix + "ProductVariants", MultiTenantProductManagementAppConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Sku).HasMaxLength(64);
            b.Property(x => x.Price).HasColumnType("decimal(18,2)");
            b.Property(x => x.Color).HasMaxLength(50);
            b.Property(x => x.Size).HasMaxLength(20);
            b.HasIndex(x => new { x.TenantId, x.ProductId, x.Sku });
        });
    }
}
