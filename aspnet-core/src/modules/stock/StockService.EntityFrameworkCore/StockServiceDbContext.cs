using Microsoft.EntityFrameworkCore;
using StockService.Stocks;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace StockService;

[ConnectionStringName("Default")]
public class StockServiceDbContext : AbpDbContext<StockServiceDbContext>
{
    public DbSet<Stock> Stocks { get; set; } = default!;
    public DbSet<StockProduct> StockProducts { get; set; } = default!;
    public DbSet<StockProductVariant> StockProductVariants { get; set; } = default!;

    public StockServiceDbContext(DbContextOptions<StockServiceDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Stock>(b =>
        {
            b.ToTable("Stocks");
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.HasMany(x => x.Products).WithOne().HasForeignKey(p => p.StockId);
        });

        builder.Entity<StockProduct>(b =>
        {
            b.ToTable("StockProducts");
            b.HasMany(x => x.Variants).WithOne().HasForeignKey(v => v.StockProductId);
        });

        builder.Entity<StockProductVariant>(b =>
        {
            b.ToTable("StockProductVariants");
        });
    }
}
