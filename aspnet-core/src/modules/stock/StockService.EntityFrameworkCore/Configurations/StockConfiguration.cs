using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockService.Stocks;

namespace StockService.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> b)
    {
        b.ToTable("Stocks");
        b.Property(x => x.Name).IsRequired().HasMaxLength(128);
        b.HasMany(x => x.Products).WithOne().HasForeignKey(p => p.StockId);
    }
}
