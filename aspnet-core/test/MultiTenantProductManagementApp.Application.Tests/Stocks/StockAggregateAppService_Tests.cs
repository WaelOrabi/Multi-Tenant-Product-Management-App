using MultiTenantProductManagementApp.Products;
using MultiTenantProductManagementApp.Stocks;
using MultiTenantProductManagementApp.Stocks.Dtos;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace MultiTenantProductManagementApp.Application.Tests.Stocks;

public class StockAggregateAppService_Tests
{
    private readonly IRepository<Stock, Guid> _stockRepo = Substitute.For<IRepository<Stock, Guid>>();
    private readonly IRepository<StockProduct, Guid> _stockProductRepo = Substitute.For<IRepository<StockProduct, Guid>>();
    private readonly IRepository<StockProductVariant, Guid> _stockProductVariantRepo = Substitute.For<IRepository<StockProductVariant, Guid>>();
    private readonly IRepository<Product, Guid> _productRepo = Substitute.For<IRepository<Product, Guid>>();
    private readonly IRepository<ProductVariant, Guid> _variantRepo = Substitute.For<IRepository<ProductVariant, Guid>>();
    private readonly ICurrentTenant _currentTenant = Substitute.For<ICurrentTenant>();
    private readonly IGuidGenerator _guidGenerator = Substitute.For<IGuidGenerator>();
    private readonly IAsyncQueryableExecuter _asyncExecuter = Substitute.For<IAsyncQueryableExecuter>();

    private StockAggregateAppService CreateService()
    {
        var svc = new StockAggregateAppService(
            _stockRepo,
            _stockProductRepo,
            _stockProductVariantRepo,
            _productRepo,
            _variantRepo
        );
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<ICurrentTenant>().Returns(_currentTenant);
        lazy.LazyGetRequiredService<IGuidGenerator>().Returns(_guidGenerator);
        lazy.LazyGetRequiredService<IAsyncQueryableExecuter>().Returns(_asyncExecuter);
        svc.LazyServiceProvider = lazy;
        return svc;
    }

    [Fact]
    public async Task GetListAsync_returns_paged_and_mapped()
    {
        // Arrange
        var sut = CreateService();
        var tenantId = Guid.NewGuid();
        _currentTenant.Id.Returns(tenantId);

        var s1 = new Stock(Guid.NewGuid(), tenantId, "S1");
        var s2 = new Stock(Guid.NewGuid(), tenantId, "S2");
        var queryable = new List<Stock> { s1, s2 }.AsQueryable();

        _stockRepo.GetQueryableAsync().Returns(Task.FromResult(queryable));
        _asyncExecuter.CountAsync(Arg.Any<IQueryable<Stock>>()).Returns(ci => Task.FromResult(ci.Arg<IQueryable<Stock>>().Count()));
        _asyncExecuter.ToListAsync(Arg.Any<IQueryable<Stock>>()).Returns(ci => Task.FromResult(ci.Arg<IQueryable<Stock>>().ToList()));



        // Act
        var result = await sut.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 10, SkipCount = 0 });

        // Assert
        result.TotalCount.ShouldBe(2);
        result.Items.Count.ShouldBe(2);
        result.Items.Any(i => i.Name == "S1").ShouldBeTrue();
    }

    [Fact]
    public async Task GetAsync_returns_detail_with_products_and_variants()
    {
        // Arrange
        var sut = CreateService();
        var tenantId = Guid.NewGuid();
        _currentTenant.Id.Returns(tenantId);

        var stockId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        _stockRepo.GetAsync(stockId).Returns(Task.FromResult(new Stock(stockId, tenantId, "Main")));

        _stockProductRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<StockProduct, bool>>>())
            .Returns(Task.FromResult(new List<StockProduct> { new StockProduct(Guid.NewGuid(), tenantId, stockId, productId) }));

        _productRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Product, bool>>>())
            .Returns(Task.FromResult(new List<Product> { new Product(productId, tenantId, "Prod-1") }));

        _stockProductVariantRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<StockProductVariant, bool>>>())
            .Returns(Task.FromResult(new List<StockProductVariant> { new StockProductVariant(Guid.NewGuid(), tenantId, Guid.NewGuid(), variantId, 3) }));

        _variantRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ProductVariant, bool>>>())
            .Returns(Task.FromResult(new List<ProductVariant> { new ProductVariant(variantId, tenantId, productId, 10m, 5, "SKU-1", "Red", "M") }));

        // Act
        var dto = await sut.GetAsync(stockId);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(stockId);
        dto.Products.Count.ShouldBe(1);
        dto.Products[0].ProductId.ShouldBe(productId);
        dto.Products[0].ProductName.ShouldBe("Prod-1");
        dto.Products[0].Variants.Count.ShouldBe(1);
        dto.Products[0].Variants[0].ProductVariantId.ShouldBe(variantId);
        dto.Products[0].Variants[0].VariantSku.ShouldBe("SKU-1");
    }

    [Fact]
    public async Task CreateAsync_inserts_root_and_children_and_returns_detail()
    {
        // Arrange
        var sut = CreateService();
        var tenantId = Guid.NewGuid();
        _currentTenant.Id.Returns(tenantId);

        var stockId = Guid.NewGuid();
        var spId = Guid.NewGuid();
        var spvId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        _guidGenerator.Create().Returns(stockId, spId, spvId);

        var input = new CreateUpdateStockAggregateDto
        {
            Name = "Main",
            Products = new List<StockProductInputDto>
            {
                new StockProductInputDto
                {
                    ProductId = productId,
                    Variants = new List<StockVariantInputDto> { new StockVariantInputDto { ProductVariantId = variantId, Quantity = 2 } }
                }
            }
        };

        _productRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Product, bool>>>())
            .Returns(Task.FromResult(new List<Product> { new Product(productId, tenantId, "P1") }));

        _variantRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ProductVariant, bool>>>())
            .Returns(Task.FromResult(new List<ProductVariant> { new ProductVariant(variantId, tenantId, productId, 1m, 5, "SKU-1", "Red", "M") }));

        _stockRepo.GetAsync(Arg.Any<Guid>()).Returns(ci => Task.FromResult(new Stock(ci.Arg<Guid>(), tenantId, "Main")));

        _stockProductRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<StockProduct, bool>>>())
            .Returns(Task.FromResult(new List<StockProduct> { new StockProduct(spId, tenantId, stockId, productId) }));

        _productRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Product, bool>>>())
            .Returns(Task.FromResult(new List<Product> { new Product(productId, tenantId, "P1") }));

        _stockProductVariantRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<StockProductVariant, bool>>>())
            .Returns(Task.FromResult(new List<StockProductVariant> { new StockProductVariant(spvId, tenantId, spId, variantId, 2) }));

        _variantRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ProductVariant, bool>>>())
            .Returns(Task.FromResult(new List<ProductVariant> { new ProductVariant(variantId, tenantId, productId, 1m, 5, "SKU-1", "Red", "M") }));

        // Act
        var dto = await sut.CreateAsync(input);

        // Assert
        await _stockRepo.Received(1).InsertAsync(Arg.Is<Stock>(s => s.Id == stockId && s.Name == "Main"), true);
        await _stockProductRepo.Received(1).InsertAsync(Arg.Is<StockProduct>(sp => sp.StockId == stockId && sp.ProductId == productId), true);
        await _stockProductVariantRepo.Received(1).InsertAsync(Arg.Is<StockProductVariant>(v => v.Quantity == 2 && v.ProductVariantId == variantId), true);
        dto.ShouldNotBeNull();
        dto.Name.ShouldBe("Main");
        dto.Products.Count.ShouldBe(1);
    }

    [Fact]
    public async Task CreateAsync_validates_input_and_throws_on_errors()
    {
        // Arrange
        var sut = CreateService();

        // Act , Assert
        await Should.ThrowAsync<BusinessException>(() => sut.CreateAsync(new CreateUpdateStockAggregateDto { Name = " ", Products = new() }));

        var negative = new CreateUpdateStockAggregateDto
        {
            Name = "Main",
            Products = new List<StockProductInputDto> { new StockProductInputDto { ProductId = Guid.NewGuid(), Variants = new List<StockVariantInputDto> { new StockVariantInputDto { ProductVariantId = null, Quantity = -1 } } } }
        };
        await Should.ThrowAsync<BusinessException>(() => sut.CreateAsync(negative));

        var dupId = Guid.NewGuid();
        var duplicate = new CreateUpdateStockAggregateDto
        {
            Name = "Main",
            Products = new List<StockProductInputDto> { new StockProductInputDto { ProductId = Guid.NewGuid(), Variants = new List<StockVariantInputDto> { new StockVariantInputDto { ProductVariantId = dupId, Quantity = 1 }, new StockVariantInputDto { ProductVariantId = dupId, Quantity = 2 } } } }
        };
        await Should.ThrowAsync<BusinessException>(() => sut.CreateAsync(duplicate));
    }

    [Fact]
    public async Task UpdateAsync_updates_children_and_returns_detail()
    {
        // Arrange
        var sut = CreateService();
        var tenantId = Guid.NewGuid();
        _currentTenant.Id.Returns(tenantId);

        var stockId = Guid.NewGuid();
        var oldSpId = Guid.NewGuid();
        var newSpId = Guid.NewGuid();
        var newSpvId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        _stockRepo.GetAsync(stockId).Returns(Task.FromResult(new Stock(stockId, tenantId, "Old")));
        _stockProductRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<StockProduct, bool>>>())
            .Returns(Task.FromResult(new List<StockProduct> { new StockProduct(oldSpId, tenantId, stockId, Guid.NewGuid()) }));

        _guidGenerator.Create().Returns(newSpId, newSpvId);

        var input = new CreateUpdateStockAggregateDto
        {
            Name = "NewName",
            Products = new List<StockProductInputDto> { new StockProductInputDto { ProductId = productId, Variants = new List<StockVariantInputDto> { new StockVariantInputDto { ProductVariantId = variantId, Quantity = 3 } } } }
        };

        _productRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Product, bool>>>())
            .Returns(Task.FromResult(new List<Product> { new Product(productId, tenantId, "P1") }));

        _variantRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ProductVariant, bool>>>())
            .Returns(Task.FromResult(new List<ProductVariant> { new ProductVariant(variantId, tenantId, productId, 1m, 10, "SKU-1", "Red", "M") }));

        _stockRepo.GetAsync(stockId).Returns(Task.FromResult(new Stock(stockId, tenantId, "NewName")));

        _stockProductRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<StockProduct, bool>>>())
            .Returns(Task.FromResult(new List<StockProduct> { new StockProduct(newSpId, tenantId, stockId, productId) }));

        _productRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Product, bool>>>())
            .Returns(Task.FromResult(new List<Product> { new Product(productId, tenantId, "P1") }));

        _stockProductVariantRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<StockProductVariant, bool>>>())
            .Returns(Task.FromResult(new List<StockProductVariant> { new StockProductVariant(newSpvId, tenantId, newSpId, variantId, 3) }));

        _variantRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ProductVariant, bool>>>())
            .Returns(Task.FromResult(new List<ProductVariant> { new ProductVariant(variantId, tenantId, productId, 1m, 10, "SKU-1", "Red", "M") }));

        // Act
        var dto = await sut.UpdateAsync(stockId, input);

        // Assert
        await _stockRepo.Received(1).UpdateAsync(Arg.Is<Stock>(s => s.Name == "NewName"), true);
        await _stockProductRepo.Received(1).DeleteAsync(Arg.Any<System.Linq.Expressions.Expression<Func<StockProduct, bool>>>());
        await _stockProductVariantRepo.Received(1).DeleteAsync(Arg.Any<System.Linq.Expressions.Expression<Func<StockProductVariant, bool>>>());
        await _stockProductRepo.Received(1).InsertAsync(Arg.Is<StockProduct>(sp => sp.ProductId == productId), true);
        await _stockProductVariantRepo.Received(1).InsertAsync(Arg.Is<StockProductVariant>(v => v.Quantity == 3), true);
        dto.Name.ShouldBe("NewName");
    }

    [Fact]
    public async Task UpdateAsync_child_error_paths_throw()
    {
        // Arrange
        var sut = CreateService();
        var tenantId = Guid.NewGuid();
        _currentTenant.Id.Returns(tenantId);

        var stockId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var otherProductId = Guid.NewGuid();

        _stockRepo.GetAsync(stockId).Returns(Task.FromResult(new Stock(stockId, tenantId, "Main")));
        _stockProductRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<StockProduct, bool>>>())
            .Returns(Task.FromResult(new List<StockProduct>()));

        _productRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Product, bool>>>())
            .Returns(Task.FromResult(new List<Product>()));
        await Should.ThrowAsync<BusinessException>(() => sut.UpdateAsync(stockId, new CreateUpdateStockAggregateDto
        {
            Name = "Main",
            Products = new List<StockProductInputDto> { new StockProductInputDto { ProductId = productId, Variants = new List<StockVariantInputDto>() } }
        }));

        _productRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Product, bool>>>())
            .Returns(Task.FromResult(new List<Product> { new Product(productId, tenantId, "P1") }));
        _variantRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ProductVariant, bool>>>())
            .Returns(Task.FromResult(new List<ProductVariant>()));
        await Should.ThrowAsync<BusinessException>(() => sut.UpdateAsync(stockId, new CreateUpdateStockAggregateDto
        {
            Name = "Main",
            Products = new List<StockProductInputDto> { new StockProductInputDto { ProductId = productId, Variants = new List<StockVariantInputDto> { new StockVariantInputDto { ProductVariantId = variantId, Quantity = 1 } } } }
        }));

        _variantRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ProductVariant, bool>>>())
            .Returns(Task.FromResult(new List<ProductVariant> { new ProductVariant(variantId, tenantId, otherProductId, 1m, 5, "SKU-1", "Red", "M") }));
        await Should.ThrowAsync<BusinessException>(() => sut.UpdateAsync(stockId, new CreateUpdateStockAggregateDto
        {
            Name = "Main",
            Products = new List<StockProductInputDto> { new StockProductInputDto { ProductId = productId, Variants = new List<StockVariantInputDto> { new StockVariantInputDto { ProductVariantId = variantId, Quantity = 1 } } } }
        }));

        _variantRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ProductVariant, bool>>>())
            .Returns(Task.FromResult(new List<ProductVariant> { new ProductVariant(variantId, tenantId, productId, 1m, 3, "SKU-1", "Red", "M") }));
        await Should.ThrowAsync<BusinessException>(() => sut.UpdateAsync(stockId, new CreateUpdateStockAggregateDto
        {
            Name = "Main",
            Products = new List<StockProductInputDto> { new StockProductInputDto { ProductId = productId, Variants = new List<StockVariantInputDto> { new StockVariantInputDto { ProductVariantId = variantId, Quantity = 10 } } } }
        }));
    }

    [Fact]
    public async Task DeleteAsync_deletes_children_then_root()
    {
        // Arrange
        var sut = CreateService();
        var tenantId = Guid.NewGuid();
        _currentTenant.Id.Returns(tenantId);

        var stockId = Guid.NewGuid();
        var sp1 = new StockProduct(Guid.NewGuid(), tenantId, stockId, Guid.NewGuid());
        var sp2 = new StockProduct(Guid.NewGuid(), tenantId, stockId, Guid.NewGuid());

        _stockRepo.GetAsync(stockId).Returns(Task.FromResult(new Stock(stockId, tenantId, "Main")));
        _stockProductRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<StockProduct, bool>>>())
            .Returns(Task.FromResult(new List<StockProduct>()));

        // Act
        await sut.DeleteAsync(stockId);

        // Assert
        await _stockProductVariantRepo.DidNotReceive().DeleteAsync(Arg.Any<System.Linq.Expressions.Expression<Func<StockProductVariant, bool>>>());
        await _stockProductRepo.Received(1).DeleteAsync(Arg.Any<System.Linq.Expressions.Expression<Func<StockProduct, bool>>>());
        await _stockRepo.Received(1).DeleteAsync(stockId);
    }
}
