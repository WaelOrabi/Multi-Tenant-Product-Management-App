using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using MongoDB.Driver;
using MultiTenantProductManagementApp.Products;
using MultiTenantProductManagementApp.Stocks;

namespace MultiTenantProductManagementApp;

[ConnectionStringName("Default")]
public class MultiTenantProductManagementAppMongoDbContext : AbpMongoDbContext
{
    // Collections
    public IMongoCollection<Product> Products => Collection<Product>();
    public IMongoCollection<ProductVariant> ProductVariants => Collection<ProductVariant>();
    public IMongoCollection<Stock> Stocks => Collection<Stock>();
    public IMongoCollection<StockProduct> StockProducts => Collection<StockProduct>();
    public IMongoCollection<StockProductVariant> StockProductVariants => Collection<StockProductVariant>();

    protected override void CreateModel(IMongoModelBuilder builder)
    {
        base.CreateModel(builder);

        builder.Entity<Product>(b =>
        {
            b.CollectionName = "Products";
        });

        builder.Entity<ProductVariant>(b =>
        {
            b.CollectionName = "ProductVariants";
        });

        builder.Entity<Stock>(b =>
        {
            b.CollectionName = "Stocks";
        });

        builder.Entity<StockProduct>(b =>
        {
            b.CollectionName = "StockProducts";
        });

        builder.Entity<StockProductVariant>(b =>
        {
            b.CollectionName = "StockProductVariants";
        });
    }
}
