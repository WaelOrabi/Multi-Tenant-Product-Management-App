using System;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Guids;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Linq;
using Volo.Abp.Domain.Entities;
using MultiTenantProductManagementApp.Products;
using MultiTenantProductManagementApp.Products.Dtos;
using Xunit;
using System.Threading;

namespace MultiTenantProductManagementApp.Application.Tests.Products;

public class ProductAppService_Tests
{
    private readonly IRepository<Product, Guid> _productRepo = Substitute.For<IRepository<Product, Guid>>();
    private readonly IRepository<ProductVariant, Guid> _variantRepo = Substitute.For<IRepository<ProductVariant, Guid>>();
    private readonly IObjectMapper _objectMapper = Substitute.For<IObjectMapper>();
    private readonly IObjectMapper<ProductAppService> _typedObjectMapper = Substitute.For<IObjectMapper<ProductAppService>>();
    private readonly ICurrentTenant _currentTenant = Substitute.For<ICurrentTenant>();
    private readonly IGuidGenerator _guidGenerator = Substitute.For<IGuidGenerator>();
    private readonly IAsyncQueryableExecuter _asyncExecuter = Substitute.For<IAsyncQueryableExecuter>();

    private ProductAppService CreateService()
    {
        var svc = new ProductAppService(_productRepo, _variantRepo);
        var lazy = Substitute.For<IAbpLazyServiceProvider>();

        lazy.LazyGetRequiredService<IObjectMapper>().Returns(_objectMapper);
        lazy.LazyGetRequiredService<IObjectMapper<ProductAppService>>().Returns(_typedObjectMapper);
        lazy.LazyGetRequiredService<ICurrentTenant>().Returns(_currentTenant);
        lazy.LazyGetRequiredService<IGuidGenerator>().Returns(_guidGenerator);
        lazy.LazyGetRequiredService<IAsyncQueryableExecuter>().Returns(_asyncExecuter);
        svc.LazyServiceProvider = lazy;
        return svc;
    }

    [Fact]
    public async Task DeleteAsync_calls_repository_delete()
    {
        var sut = CreateService();
        var id = Guid.NewGuid();
        await sut.DeleteAsync(id);
        await _productRepo.Received(1).DeleteAsync(id);
    }

    [Fact]
    public async Task UpdateVariantAsync_throws_when_product_mismatch()
    {
        var sut = CreateService();
        var productId = Guid.NewGuid();
        var otherProductId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var variant = new ProductVariant(variantId, null, otherProductId, 10m, 5, "SKU-1", "Red", "L");
        _variantRepo.GetAsync(variantId).Returns(Task.FromResult(variant));

        var input = new CreateUpdateProductVariantDto { Price = 12m, StockQuantity = 7, Sku = "SKU-2", Color = "Blue", Size = "M" };
        var ex = await Should.ThrowAsync<BusinessException>(() => sut.UpdateVariantAsync(productId, variantId, input));
        ex.Code.ShouldBe("ProductVariant.ProductMismatch");
        ex.Data["ProductId"].ShouldBe(productId);
        ex.Data["VariantId"].ShouldBe(variantId);
    }

    [Fact]
    public async Task AddVariantAsync_inserts_and_maps_variant()
    {
        var sut = CreateService();
        var productId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var newVariantId = Guid.NewGuid();
        _currentTenant.Id.Returns(tenantId);
        _guidGenerator.Create().Returns(newVariantId);

        var product = new Product(productId, tenantId, "Name", "Desc", 100m, "Cat", ProductStatus.Active, hasVariants: false);
        _productRepo.GetAsync(productId).Returns(Task.FromResult(product));

        var input = new CreateUpdateProductVariantDto { Price = 15m, StockQuantity = 3, Sku = "SKU-3", Color = "Black", Size = "S" };

        var dto = await sut.AddVariantAsync(productId, input);

        await _variantRepo.Received(1).InsertAsync(Arg.Is<ProductVariant>(v =>
            v.ProductId == productId &&
            v.Price == input.Price &&
            v.StockQuantity == input.StockQuantity &&
            v.Sku == input.Sku &&
            v.Color == input.Color &&
            v.Size == input.Size
        ), true);

        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(newVariantId);
        dto.ProductId.ShouldBe(productId);
        dto.Sku.ShouldBe("SKU-3");
    }

    [Fact]
    public async Task CreateAsync_inserts_product_and_maps()
    {
        var sut = CreateService();
        var tenantId = Guid.NewGuid();
        var generatedId = Guid.NewGuid();
        _currentTenant.Id.Returns(tenantId);
        _guidGenerator.Create().Returns(generatedId);

        var input = new CreateUpdateProductDto
        {
            Name = "Prod-1",
            Description = "Desc-1",
            BasePrice = 50m,
            Category = "Cat-1",
            Status = ProductStatus.Active,
            HasVariants = true,
            Variants = new List<CreateUpdateProductVariantDto> { new() { Price = 55m, StockQuantity = 2, Sku = "SKU-A", Color = "Red", Size = "M" } }
        };

        var dto = await sut.CreateAsync(input);

        await _productRepo.Received(1).InsertAsync(Arg.Is<Product>(p =>
            p.TenantId == tenantId &&
            p.Name == input.Name &&
            p.HasVariants &&
            p.Variants.Count == 1 &&
            p.Variants.First().Sku == "SKU-A"
        ), true);

        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(generatedId);
        dto.Name.ShouldBe("Prod-1");
    }

    [Fact]
    public async Task UpdateAsync_updates_fields_and_variants_then_maps()
    {
        var sut = CreateService();
        var tenantId = Guid.NewGuid();
        var id = Guid.NewGuid();
        _currentTenant.Id.Returns(tenantId);

        var existing = new Product(id, tenantId, "OldName", "OldDesc", 10m, "OldCat", ProductStatus.Inactive, hasVariants: true);
        existing.Variants.Add(new ProductVariant(Guid.NewGuid(), tenantId, id, 9m, 1, "OLD", "Green", "S"));

        var data = new List<Product> { existing };
        var queryable = data.AsQueryable();

        _productRepo.WithDetailsAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Product, object>>[]>()).Returns(Task.FromResult(queryable));
        _asyncExecuter.FirstOrDefaultAsync(Arg.Any<IQueryable<Product>>()).Returns(Task.FromResult(existing));

        var input = new CreateUpdateProductDto
        {
            Name = "NewName",
            Description = "NewDesc",
            BasePrice = 20m,
            Category = "NewCat",
            Status = ProductStatus.Active,
            HasVariants = true,
            Variants = new List<CreateUpdateProductVariantDto> { new() { Price = 22m, StockQuantity = 5, Sku = "NEW", Color = "Blue", Size = "M" } }
        };

        var dto = await sut.UpdateAsync(id, input);

        await _productRepo.Received(1).UpdateAsync(Arg.Is<Product>(p =>
            p.Name == "NewName" &&
            p.Description == "NewDesc" &&
            p.BasePrice == 20m &&
            p.Category == "NewCat" &&
            p.Status == ProductStatus.Active &&
            p.Variants != null &&
            p.Variants.Count == 1 &&
            p.Variants.Any(v => v.Sku == "NEW")
        ), true);

        dto.ShouldNotBeNull();
        dto.Name.ShouldBe("NewName");
    }

    [Fact]
    public async Task GetListAsync_applies_filter_and_sorting_and_maps()
    {
        var sut = CreateService();
        var tenantId = Guid.NewGuid();
        _currentTenant.Id.Returns(tenantId);

        var p1 = new Product(Guid.NewGuid(), tenantId, "Alpha", "DescA", 10m, "Cat1", ProductStatus.Active, false);
        var p2 = new Product(Guid.NewGuid(), tenantId, "Beta", "DescB", 20m, "Cat2", ProductStatus.Inactive, false);
        var data = new List<Product> { p1, p2 };
        var queryable = data.AsQueryable();

        _productRepo.WithDetailsAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Product, object>>[]>()).Returns(Task.FromResult(queryable));
        _asyncExecuter.CountAsync(Arg.Any<IQueryable<Product>>()).Returns(ci => Task.FromResult(ci.Arg<IQueryable<Product>>().Count()));
        _asyncExecuter.ToListAsync(Arg.Any<IQueryable<Product>>()).Returns(ci => Task.FromResult(ci.Arg<IQueryable<Product>>().ToList()));

        var input = new GetProductListInput { FilterText = string.Empty, Sorting = "Name desc", MaxResultCount = 10, SkipCount = 0 };
        var result = await sut.GetListAsync(input);

        result.ShouldNotBeNull();
        result.TotalCount.ShouldBe(2);
        result.Items.ShouldNotBeNull();
        result.Items.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetAsync_returns_product_with_variants_and_notfound()
    {
        var sut = CreateService();
        var tenantId = Guid.NewGuid();
        var id = Guid.NewGuid();
        _currentTenant.Id.Returns(tenantId);

        var product = new Product(id, tenantId, "ProdX", "DescX", 12m, "CatX", ProductStatus.Active, hasVariants: true);
        product.Variants.Add(new ProductVariant(Guid.NewGuid(), tenantId, id, 13m, 4, "SKU-X", "Black", "L"));

        var data = new List<Product> { product };
        var queryable = data.AsQueryable();
        _productRepo.WithDetailsAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Product, object>>[]>()).Returns(Task.FromResult(queryable));
        _asyncExecuter.FirstOrDefaultAsync(Arg.Any<IQueryable<Product>>()).Returns(Task.FromResult(product));

        var dto = await sut.GetAsync(id);
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(id);
        dto.Variants.Count.ShouldBe(1);

    
        _asyncExecuter.FirstOrDefaultAsync(Arg.Any<IQueryable<Product>>()).Returns(Task.FromResult((Product)null));
        await Should.ThrowAsync<EntityNotFoundException>(() => sut.GetAsync(Guid.NewGuid()));
    }
}
