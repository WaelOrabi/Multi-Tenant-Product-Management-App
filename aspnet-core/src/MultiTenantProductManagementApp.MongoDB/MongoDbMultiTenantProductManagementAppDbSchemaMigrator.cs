using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using MultiTenantProductManagementApp.Data;
using Volo.Abp.DependencyInjection;

namespace MultiTenantProductManagementApp;

public class MongoDbMultiTenantProductManagementAppDbSchemaMigrator : IMultiTenantProductManagementAppDbSchemaMigrator, ITransientDependency
{
    private readonly MultiTenantProductManagementAppMongoDbContext _mongoContext;

    public MongoDbMultiTenantProductManagementAppDbSchemaMigrator(MultiTenantProductManagementAppMongoDbContext mongoContext)
    {
        _mongoContext = mongoContext;
    }

    public async Task MigrateAsync()
    {
        // Ensure collections exist (no-op if already created)
        _ = _mongoContext.Products;
        _ = _mongoContext.ProductVariants;
        _ = _mongoContext.Stocks;
        _ = _mongoContext.StockProducts;
        _ = _mongoContext.StockProductVariants;

        // Data migration: Move Color/Size to Options on the raw documents and unset old fields
        var rawCollection = _mongoContext.ProductVariants.Database.GetCollection<BsonDocument>("ProductVariants");
        var rawFilter = Builders<BsonDocument>.Filter.Or(
            Builders<BsonDocument>.Filter.Exists("Color", true),
            Builders<BsonDocument>.Filter.Exists("Size", true)
        );

        using var cursor = await rawCollection.FindAsync(rawFilter);
        while (await cursor.MoveNextAsync())
        {
            foreach (var doc in cursor.Current)
            {
                var optionsArray = new BsonArray();
                if (doc.TryGetValue("Color", out var colorVal) && colorVal.IsString && !string.IsNullOrWhiteSpace(colorVal.AsString))
                {
                    optionsArray.Add(new BsonDocument { { "Name", "Color" }, { "Value", colorVal.AsString } });
                }
                if (doc.TryGetValue("Size", out var sizeVal) && sizeVal.IsString && !string.IsNullOrWhiteSpace(sizeVal.AsString))
                {
                    optionsArray.Add(new BsonDocument { { "Name", "Size" }, { "Value", sizeVal.AsString } });
                }

                var update = Builders<BsonDocument>.Update
                    .Set("Options", optionsArray)
                    .Unset("Color")
                    .Unset("Size");

                await rawCollection.UpdateOneAsync(
                    Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]),
                    update
                );
            }
        }
    }
}
