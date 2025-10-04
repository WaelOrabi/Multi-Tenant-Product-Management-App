using MultiTenantProductManagementApp.Products;
using MultiTenantProductManagementApp.Products.Dtos;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace MultiTenantProductManagementApp.Application.Tests.Products;

public class ProductAppService_Tests
{
    private readonly IRepository<Product, Guid> _productRepo = Substitute.For<IRepository<Product, Guid>>();
    private readonly IRepository<ProductVariant, Guid> _variantRepo = Substitute.For<IRepository<ProductVariant, Guid>>();
    private readonly IObjectMapper<ProductAppService> _typedObjectMapper = Substitute.For<IObjectMapper<ProductAppService>>();
    private readonly ICurrentTenant _currentTenant = Substitute.For<ICurrentTenant>();
    private readonly IGuidGenerator _guidGenerator = Substitute.For<IGuidGenerator>();
    private readonly IAsyncQueryableExecuter _asyncExecuter = Substitute.For<IAsyncQueryableExecuter>();

    private ProductAppService CreateService()
    {
        var svc = new ProductAppService(_productRepo, _variantRepo);
        var lazy = Substitute.For<IAbpLazyServiceProvider>();

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
        // Arrange
        var sut = CreateService();
        var id = Guid.NewGuid();

        // Act
        await sut.DeleteAsync(id);

        // Assert
        await _productRepo.Received(1).DeleteAsync(id);
    }

    [Fact]
    public async Task UpdateVariantAsync_throws_when_product_mismatch()
    {
        // Arrange
        var sut = CreateService();
        var productId = Guid.NewGuid();
        var otherProductId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var variant = new ProductVariant(variantId, null, otherProductId, 10m, "SKU-1", new[] { new ProductVariantOption("Color", "Red"), new ProductVariantOption("Size", "L") });
        _variantRepo.GetAsync(variantId).Returns(Task.FromResult(variant));

        var input = new CreateUpdateProductVariantDto { Price = 12m, Sku = "SKU-2", Options = new List<ProductVariantOptionDto> { new() { Name = "Color", Value = "Blue" }, new() { Name = "Size", Value = "M" } } };

        // Act 
        var ex = await Should.ThrowAsync<BusinessException>(() => sut.UpdateVariantAsync(productId, variantId, input));


        ex.Code.ShouldBe("ProductVariant.ProductMismatch");
        ex.Data["ProductId"].ShouldBe(productId);
        ex.Data["VariantId"].ShouldBe(variantId);
    }


    [Fact]
    public async Task AddVariantAsync_inserts_and_maps_variant()
    {
        // Arrange
        var sut = CreateService();
        var productId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var newVariantId = Guid.NewGuid();
        _currentTenant.Id.Returns(tenantId);
        _guidGenerator.Create().Returns(newVariantId);

        var product = new Product(productId, tenantId, "Name", "Desc", 100m, "Cat", ProductStatus.Active, hasVariants: false);
        _productRepo.GetAsync(productId).Returns(Task.FromResult(product));

        var input = new CreateUpdateProductVariantDto { Price = 15m, Sku = "SKU-3", Options = new List<ProductVariantOptionDto> { new() { Name = "Color", Value = "Black" }, new() { Name = "Size", Value = "S" } } };

        // Act
        var dto = await sut.AddVariantAsync(productId, input);

        // Assert
        await _variantRepo.Received(1).InsertAsync(Arg.Is<ProductVariant>(v =>
            v.ProductId == productId &&
            v.Price == input.Price &&
            v.Sku == input.Sku &&
            v.Options.Any(o => o.Name == "Color" && o.Value == "Black") &&
            v.Options.Any(o => o.Name == "Size" && o.Value == "S")
        ), true);

        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(newVariantId);
        dto.ProductId.ShouldBe(productId);
        dto.Sku.ShouldBe("SKU-3");
    }

    [Fact]
    public async Task CreateAsync_inserts_product_and_maps()
    {
        // Arrange
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
            Variants = new List<CreateUpdateProductVariantDto> { new() { Price = 55m, Sku = "SKU-A", Options = new List<ProductVariantOptionDto> { new() { Name = "Color", Value = "Red" }, new() { Name = "Size", Value = "M" } } } }
        };

        // Act
        var dto = await sut.CreateAsync(input);

        // Assert
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
        // Arrange
        var sut = CreateService();
        var tenantId = Guid.NewGuid();
        var id = Guid.NewGuid();
        _currentTenant.Id.Returns(tenantId);

        var existing = new Product(id, tenantId, "OldName", "OldDesc", 10m, "OldCat", ProductStatus.Inactive, hasVariants: true);
        existing.Variants.Add(new ProductVariant(Guid.NewGuid(), tenantId, id, 9m, "OLD", new[] { new ProductVariantOption("Color", "Green"), new ProductVariantOption("Size", "S") }));

        var data = new List<Product> { existing };
        var queryable = data.AsQueryable();

        _productRepo.WithDetailsAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Product, object>>[]>()).Returns(Task.FromResult(queryable));
        _asyncExecuter.FirstOrDefaultAsync(Arg.Any<IQueryable<Product>>()).Returns(Task.FromResult<Product?>(existing));

        var input = new CreateUpdateProductDto
        {
            Name = "NewName",
            Description = "NewDesc",
            BasePrice = 20m,
            Category = "NewCat",
            Status = ProductStatus.Active,
            HasVariants = true,
            Variants = new List<CreateUpdateProductVariantDto> { new() { Price = 22m, Sku = "NEW", Options = new List<ProductVariantOptionDto> { new() { Name = "Color", Value = "Blue" }, new() { Name = "Size", Value = "M" } } } }
        };

        // Act
        var dto = await sut.UpdateAsync(id, input);

        // Assert
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
        // Arrange
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

        // Act
        var result = await sut.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBe(2);
        result.Items.ShouldNotBeNull();
        result.Items.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetAsync_returns_product_with_variants_and_notfound()
    {
        // Arrange
        var sut = CreateService();
        var tenantId = Guid.NewGuid();
        var id = Guid.NewGuid();
        _currentTenant.Id.Returns(tenantId);

        var product = new Product(id, tenantId, "ProdX", "DescX", 12m, "CatX", ProductStatus.Active, hasVariants: true);
        product.Variants.Add(new ProductVariant(Guid.NewGuid(), tenantId, id, 13m, "SKU-X", new[] { new ProductVariantOption("Color", "Black"), new ProductVariantOption("Size", "L") }));

        var data = new List<Product> { product };
        var queryable = data.AsQueryable();
        _productRepo.WithDetailsAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Product, object>>[]>()).Returns(Task.FromResult(queryable));
        _asyncExecuter.FirstOrDefaultAsync(Arg.Any<IQueryable<Product>>()).Returns(Task.FromResult<Product?>(product));

        // Act
        var dto = await sut.GetAsync(id);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(id);
        dto.Variants.Count.ShouldBe(1);

        _asyncExecuter.FirstOrDefaultAsync(Arg.Any<IQueryable<Product>>()).Returns(Task.FromResult<Product?>(null));
        await Should.ThrowAsync<EntityNotFoundException>(() => sut.GetAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateVariantAsync_updates_and_maps_successfully()
    {
        // Arrange
        var sut = CreateService();
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var variant = new ProductVariant(variantId, tenantId, productId, 10m, "OLD", new[] { new ProductVariantOption("Color", "Red"), new ProductVariantOption("Size", "L") });
        _variantRepo.GetAsync(variantId).Returns(Task.FromResult(variant));

        var input = new CreateUpdateProductVariantDto { Price = 20m, Sku = "NEW", Options = new List<ProductVariantOptionDto> { new() { Name = "Color", Value = "Blue" }, new() { Name = "Size", Value = "M" } } };

        // Act
        var dto = await sut.UpdateVariantAsync(productId, variantId, input);

        // Assert
        await _variantRepo.Received(1).UpdateAsync(Arg.Is<ProductVariant>(v =>
            v.Id == variantId &&
            v.ProductId == productId &&
            v.Sku == "NEW" &&
            v.Options.Any(o => o.Name == "Color" && o.Value == "Blue") &&
            v.Options.Any(o => o.Name == "Size" && o.Value == "M") &&
            v.Price == 20m
        ), true);
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(variantId);
        dto.Sku.ShouldBe("NEW");
    }

    [Fact]
    public async Task DeleteVariantAsync_throws_when_product_mismatch()
    {
        // Arrange
        var sut = CreateService();
        var productId = Guid.NewGuid();
        var otherProductId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var variant = new ProductVariant(variantId, null, otherProductId, 10m, "SKU-1", new[] { new ProductVariantOption("Color", "Red"), new ProductVariantOption("Size", "L") });
        _variantRepo.GetAsync(variantId).Returns(Task.FromResult(variant));

        // Act , Assert
        var ex = await Should.ThrowAsync<BusinessException>(() => sut.DeleteVariantAsync(productId, variantId));
        ex.Code.ShouldBe("ProductVariant.ProductMismatch");
        ex.Data["ProductId"].ShouldBe(productId);
        ex.Data["VariantId"].ShouldBe(variantId);
    }

    [Fact]
    public async Task DeleteVariantAsync_deletes_when_matches()
    {
        // Arrange
        var sut = CreateService();
        var tenantId = Guid.NewGuid();
        _currentTenant.Id.Returns(tenantId);
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var variant = new ProductVariant(variantId, tenantId, productId, 10m, "SKU-1", new[] { new ProductVariantOption("Color", "Red"), new ProductVariantOption("Size", "L") });
        _variantRepo.GetAsync(variantId).Returns(Task.FromResult(variant));

        // Act
        await sut.DeleteVariantAsync(productId, variantId);

        // Assert
        await _variantRepo.Received(1).DeleteAsync(Arg.Is<ProductVariant>(v => v.Id == variantId));
    }

    [Fact]
    public async Task GetListAsync_applies_specific_filters_and_default_sorting()
    {
        // Arrange
        var sut = CreateService();
        var tenantId = Guid.NewGuid();
        _currentTenant.Id.Returns(tenantId);

        var p1 = new Product(Guid.NewGuid(), tenantId, "Alpha", "DescA", 10m, "Cat1", ProductStatus.Active, false);
        var p2 = new Product(Guid.NewGuid(), tenantId, "Beta", "DescB", 20m, "Cat2", ProductStatus.Inactive, false);
        var p3 = new Product(Guid.NewGuid(), tenantId, "Gamma", "DescG", 30m, "Cat1", ProductStatus.Active, false);
        var data = new List<Product> { p1, p2, p3 };
        var queryable = data.AsQueryable();

        _productRepo.WithDetailsAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Product, object>>[]>()).Returns(Task.FromResult(queryable));
        _asyncExecuter.CountAsync(Arg.Any<IQueryable<Product>>()).Returns(ci => Task.FromResult(ci.Arg<IQueryable<Product>>().Count()));
        _asyncExecuter.ToListAsync(Arg.Any<IQueryable<Product>>()).Returns(ci => Task.FromResult(ci.Arg<IQueryable<Product>>().ToList()));

        // Act
        var result = await sut.GetListAsync(new GetProductListInput
        {
            FilterText = "a",
            Name = "a",
            Category = "Cat1",
            Status = ProductStatus.Active,
            Sorting = null,
            SkipCount = 0,
            MaxResultCount = 10
        });

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBe(2);
        result.Items.Count.ShouldBe(2);
        result.Items.All(d => d.Category == "Cat1" && d.Status == ProductStatus.Active).ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateAsync_throws_when_entity_not_found()
    {
        // Arrange
        var sut = CreateService();
        var id = Guid.NewGuid();
        var emptyQueryable = new List<Product>().AsQueryable();
        _productRepo.WithDetailsAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Product, object>>[]>()).Returns(Task.FromResult(emptyQueryable));
        _asyncExecuter.FirstOrDefaultAsync(Arg.Any<IQueryable<Product>>()).Returns(Task.FromResult<Product?>(null));

        var input = new CreateUpdateProductDto { Name = "X" };

        // Act , Assert
        await Should.ThrowAsync<EntityNotFoundException>(() => sut.UpdateAsync(id, input));
    }

    [Fact]
    public async Task CreateAsync_without_variants_inserts_basic_product()
    {
        // Arrange
        var sut = CreateService();
        var tenantId = Guid.NewGuid();
        _currentTenant.Id.Returns(tenantId);
        var newId = Guid.NewGuid();
        _guidGenerator.Create().Returns(newId);

        var input = new CreateUpdateProductDto
        {
            Name = "Simple",
            Description = "No variants",
            BasePrice = 9.99m,
            Category = "Misc",
            Status = ProductStatus.Active,
            HasVariants = false,
            Variants = new List<CreateUpdateProductVariantDto>()
        };

        // Act
        var dto = await sut.CreateAsync(input);

        // Assert
        await _productRepo.Received(1).InsertAsync(Arg.Is<Product>(p =>
            p.Name == "Simple" &&
            !p.HasVariants &&
            (p.Variants == null || p.Variants.Count == 0)
        ), true);
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(newId);
        dto.Name.ShouldBe("Simple");
    }
}
