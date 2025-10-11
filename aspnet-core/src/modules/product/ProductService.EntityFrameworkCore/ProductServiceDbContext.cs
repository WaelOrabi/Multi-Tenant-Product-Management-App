using Microsoft.EntityFrameworkCore;
using ProductService.Products;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace ProductService;

[ConnectionStringName("Default")]
public class ProductServiceDbContext : AbpDbContext<ProductServiceDbContext>
{
    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<ProductVariant> ProductVariants { get; set; } = default!;

    public ProductServiceDbContext(DbContextOptions<ProductServiceDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Product>(b =>
        {
            b.ToTable("Products");
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Category).HasMaxLength(64);
            b.HasMany(x => x.Variants).WithOne().HasForeignKey(v => v.ProductId);
        });

        builder.Entity<ProductVariant>(b =>
        {
            b.ToTable("ProductVariants");
            b.Property(x => x.Sku).HasMaxLength(64);

            // Map owned options as separate table with PK
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

