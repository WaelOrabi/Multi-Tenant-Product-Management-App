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

        // Load IEntityTypeConfiguration<> from this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(ProductServiceDbContext).Assembly);
    }
}

