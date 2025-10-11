using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockService.Stocks;

namespace StockService.Configurations;

public class StockProductConfiguration : IEntityTypeConfiguration<StockProduct>
{
    public void Configure(EntityTypeBuilder<StockProduct> b)
    {
        b.ToTable("StockProducts");
        b.HasMany(x => x.Variants).WithOne().HasForeignKey(v => v.StockProductId);
    }
}
