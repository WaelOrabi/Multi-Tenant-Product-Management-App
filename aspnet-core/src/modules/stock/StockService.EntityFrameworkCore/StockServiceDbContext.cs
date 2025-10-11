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

        // Load IEntityTypeConfiguration<> from this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(StockServiceDbContext).Assembly);
    }
}
