using Microsoft.EntityFrameworkCore;
using StockService.Stocks;

namespace StockService.Extensions;

public static class StockServiceModelBuilderExtensions
{
    public static void ConfigureStockService(this ModelBuilder builder)
    {
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
