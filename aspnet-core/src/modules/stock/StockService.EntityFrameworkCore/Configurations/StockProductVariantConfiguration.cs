using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockService.Stocks;

namespace StockService.Configurations;

public class StockProductVariantConfiguration : IEntityTypeConfiguration<StockProductVariant>
{
    public void Configure(EntityTypeBuilder<StockProductVariant> b)
    {
        b.ToTable("StockProductVariants");
    }
}
