using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MultiTenantProductManagementApp.EntityFrameworkCore;
using MultiTenantProductManagementApp.Products;
using MultiTenantProductManagementApp.Stocks;
using MultiTenantProductManagementApp.Stocks.Dtos;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace MultiTenantProductManagementApp.Application.Tests.Stocks;

public class StockAggregateAppService_Integration_Tests : MultiTenantProductManagementAppEntityFrameworkCoreTestBase, IAsyncLifetime
{
    private IStockAggregateAppService _stockAppService = default!;
    private IRepository<Stock, Guid> _stockRepo = default!;
    private IRepository<StockProduct, Guid> _stockProductRepo = default!;
    private IRepository<StockProductVariant, Guid> _stockProductVariantRepo = default!;
    private IRepository<Product, Guid> _productRepo = default!;
    private IRepository<ProductVariant, Guid> _variantRepo = default!;
    private IDataFilter _dataFilter = default!;

    private static readonly Guid _tenantId = Guid.NewGuid();
    private IDisposable? _tenantScope;

    private async Task InTenantAsync(Func<Task> action)
    {
        var currentTenant = GetRequiredService<ICurrentTenant>();
        using (currentTenant.Change(_tenantId))
        {
            await action();
        }
    }

    private Product _testProduct1 = default!;
    private Product _testProduct2 = default!;
    private ProductVariant _testVariant1 = default!;
    private ProductVariant _testVariant2 = default!;
    private ProductVariant _testVariant3 = default!;

    public async Task InitializeAsync()
    {
        var currentTenant = GetRequiredService<ICurrentTenant>();
        _tenantScope = currentTenant.Change(_tenantId);

        _stockAppService = GetRequiredService<IStockAggregateAppService>();
        _stockRepo = GetRequiredService<IRepository<Stock, Guid>>();
        _stockProductRepo = GetRequiredService<IRepository<StockProduct, Guid>>();
        _stockProductVariantRepo = GetRequiredService<IRepository<StockProductVariant, Guid>>();
        _productRepo = GetRequiredService<IRepository<Product, Guid>>();
        _variantRepo = GetRequiredService<IRepository<ProductVariant, Guid>>();
        _dataFilter = GetRequiredService<IDataFilter>();

        var dbContext = GetRequiredService<MultiTenantProductManagementAppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        await CleanupTestDataAsync();

        await InTenantAsync(async () =>
        {
            await SeedTestDataAsync();
        });
    }

    public async Task DisposeAsync()
    {
        await CleanupTestDataAsync();
        _tenantScope?.Dispose();
    }

private async Task SeedTestDataAsync()
{
    await WithUnitOfWorkAsync(async () =>
    {
     
        await CleanupTestDataAsync();

        _testProduct1 = new Product(
            Guid.NewGuid(),
            _tenantId,
            "Test Phone",
            "Test smartphone",
            999.99m,
            "Electronics",
            ProductStatus.Active,
            true
        );
        await _productRepo.InsertAsync(_testProduct1);

        _testProduct2 = new Product(
            Guid.NewGuid(),
            _tenantId,
            "Test Laptop",
            "Test laptop computer",
            1499.99m,
            "Electronics",
            ProductStatus.Active,
            true
        );
        await _productRepo.InsertAsync(_testProduct2);

      
        _testVariant1 = new ProductVariant(
            Guid.NewGuid(),
            _tenantId,
            _testProduct1.Id,
            1199.99m,
            10,
            "PHONE-BLK-128",
            "Black",
            "128GB"
        );
        await _variantRepo.InsertAsync(_testVariant1);

        _testVariant2 = new ProductVariant(
            Guid.NewGuid(),
            _tenantId,
            _testProduct1.Id,
            1299.99m,
            5,
            "PHONE-SLV-256",
            "Silver",
            "256GB"
        );
        await _variantRepo.InsertAsync(_testVariant2);

        _testVariant3 = new ProductVariant(
            Guid.NewGuid(),
            _tenantId,
            _testProduct2.Id,
            1699.99m,
            8,
            "LAPTOP-GRY-16",
            "Gray",
            "16GB"
        );
        await _variantRepo.InsertAsync(_testVariant3);
    });
}

private async Task CleanupTestDataAsync()
{
    await WithUnitOfWorkAsync(async () =>
    {
   
        var stockProductVariants = await _stockProductVariantRepo.GetListAsync();
        foreach (var spv in stockProductVariants)
        {
            await _stockProductVariantRepo.DeleteAsync(spv);
        }

        var stockProducts = await _stockProductRepo.GetListAsync();
        foreach (var sp in stockProducts)
        {
            await _stockProductRepo.DeleteAsync(sp);
        }

        var stocks = await _stockRepo.GetListAsync();
        foreach (var s in stocks)
        {
            await _stockRepo.DeleteAsync(s);
        }

        var variants = await _variantRepo.GetListAsync();
        foreach (var v in variants)
        {
            await _variantRepo.DeleteAsync(v);
        }

        var products = await _productRepo.GetListAsync();
        foreach (var p in products)
        {
            await _productRepo.DeleteAsync(p);
        }
    });
}

   

    [Fact]
    public async Task CreateAsync_Should_Create_Stock_With_Products_And_Variants()
    {
        await InTenantAsync(async () =>
        {
            // Arrange
            var input = new CreateUpdateStockAggregateDto
            {
                Name = "Test Stock 1",
                Products = new List<StockProductInputDto>
                {
                    new()
                    {
                        ProductId = _testProduct1.Id,
                        Variants = new List<StockVariantInputDto>
                        {
                            new() { ProductVariantId = _testVariant1.Id, Quantity = 3 },
                            new() { ProductVariantId = _testVariant2.Id, Quantity = 2 }
                        }
                    },
                    new()
                    {
                        ProductId = _testProduct2.Id,
                        Variants = new List<StockVariantInputDto>
                        {
                            new() { ProductVariantId = _testVariant3.Id, Quantity = 1 }
                        }
                    }
                }
            };

            // Act
            var result = await _stockAppService.CreateAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldNotBe(Guid.Empty);
            result.Name.ShouldBe("Test Stock 1");
            result.Products.Count.ShouldBe(2);

            var product1 = result.Products.First(p => p.ProductId == _testProduct1.Id);
            product1.ProductName.ShouldBe("Test Phone");
            product1.Variants.Count.ShouldBe(2);
            product1.Variants.Any(v => v.ProductVariantId == _testVariant1.Id && v.Quantity == 3).ShouldBeTrue();
            product1.Variants.Any(v => v.ProductVariantId == _testVariant2.Id && v.Quantity == 2).ShouldBeTrue();

            var product2 = result.Products.First(p => p.ProductId == _testProduct2.Id);
            product2.ProductName.ShouldBe("Test Laptop");
            product2.Variants.Count.ShouldBe(1);
            product2.Variants.First().ProductVariantId.ShouldBe(_testVariant3.Id);
            product2.Variants.First().Quantity.ShouldBe(1);

      
            await WithUnitOfWorkAsync(async () =>
            {
                var stock = await _stockRepo.GetAsync(result.Id);
                stock.Name.ShouldBe("Test Stock 1");

                var stockProducts = await _stockProductRepo.GetListAsync(sp => sp.StockId == stock.Id);
                stockProducts.Count.ShouldBe(2);

                var stockVariants = await _stockProductVariantRepo.GetListAsync();
                var relevantVariants = stockVariants.Where(sv => stockProducts.Any(sp => sp.Id == sv.StockProductId)).ToList();
                relevantVariants.Count.ShouldBe(3);
            });
        });
    }

    [Fact]
    public async Task GetAsync_Should_Return_Complete_Stock_Details()
    {
        await InTenantAsync(async () =>
        {
            // Arrange
            var createInput = new CreateUpdateStockAggregateDto
            {
                Name = "Test Stock for Get",
                Products = new List<StockProductInputDto>
                {
                    new()
                    {
                        ProductId = _testProduct1.Id,
                        Variants = new List<StockVariantInputDto>
                        {
                            new() { ProductVariantId = _testVariant1.Id, Quantity = 5 }
                        }
                    }
                }
            };
            var created = await _stockAppService.CreateAsync(createInput);

            // Act
            var result = await _stockAppService.GetAsync(created.Id);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(created.Id);
            result.Name.ShouldBe("Test Stock for Get");
            result.Products.Count.ShouldBe(1);

            var product = result.Products.First();
            product.ProductId.ShouldBe(_testProduct1.Id);
            product.ProductName.ShouldBe("Test Phone");
            product.Variants.Count.ShouldBe(1);

            var variant = product.Variants.First();
            variant.ProductVariantId.ShouldBe(_testVariant1.Id);
            variant.Quantity.ShouldBe(5);
            variant.VariantSku.ShouldBe("PHONE-BLK-128");
            variant.Color.ShouldBe("Black");
            variant.Size.ShouldBe("128GB");
        });
    }

    [Fact]
    public async Task GetListAsync_Should_Return_Paged_Stock_Summary()
    {
        await InTenantAsync(async () =>
        {
            // Arrange
            await _stockAppService.CreateAsync(new CreateUpdateStockAggregateDto
            {
                Name = "Stock A",
                Products = new List<StockProductInputDto>
                {
                    new() { ProductId = _testProduct1.Id, Variants = new List<StockVariantInputDto>() }
                }
            });

            await _stockAppService.CreateAsync(new CreateUpdateStockAggregateDto
            {
                Name = "Stock B",
                Products = new List<StockProductInputDto>
                {
                    new() { ProductId = _testProduct2.Id, Variants = new List<StockVariantInputDto>() }
                }
            });

            // Act
            var result = await _stockAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0
            });

            // Assert
            result.ShouldNotBeNull();
            result.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
            result.Items.Count.ShouldBeGreaterThanOrEqualTo(2);
            result.Items.Any(s => s.Name == "Stock A").ShouldBeTrue();
            result.Items.Any(s => s.Name == "Stock B").ShouldBeTrue();
        });
    }

    [Fact]
    public async Task UpdateAsync_Should_Replace_Products_And_Variants()
    {
        await InTenantAsync(async () =>
        {
            // Arrange 
            var createInput = new CreateUpdateStockAggregateDto
            {
                Name = "Original Stock",
                Products = new List<StockProductInputDto>
                {
                    new()
                    {
                        ProductId = _testProduct1.Id,
                        Variants = new List<StockVariantInputDto>
                        {
                            new() { ProductVariantId = _testVariant1.Id, Quantity = 3 }
                        }
                    }
                }
            };
            var created = await _stockAppService.CreateAsync(createInput);

            // Act 
            var updateInput = new CreateUpdateStockAggregateDto
            {
                Name = "Updated Stock",
                Products = new List<StockProductInputDto>
                {
                    new()
                    {
                        ProductId = _testProduct2.Id,
                        Variants = new List<StockVariantInputDto>
                        {
                            new() { ProductVariantId = _testVariant3.Id, Quantity = 7 }
                        }
                    }
                }
            };
            var updated = await _stockAppService.UpdateAsync(created.Id, updateInput);

            // Assert
            updated.Name.ShouldBe("Updated Stock");
            updated.Products.Count.ShouldBe(1);
            updated.Products.First().ProductId.ShouldBe(_testProduct2.Id);
            updated.Products.First().Variants.First().ProductVariantId.ShouldBe(_testVariant3.Id);
            updated.Products.First().Variants.First().Quantity.ShouldBe(7);

         
            await WithUnitOfWorkAsync(async () =>
            {
                var stockProducts = await _stockProductRepo.GetListAsync(sp => sp.StockId == created.Id);
                stockProducts.Count.ShouldBe(1);
                stockProducts.First().ProductId.ShouldBe(_testProduct2.Id);

                var stockVariants = await _stockProductVariantRepo.GetListAsync(sv => sv.StockProductId == stockProducts.First().Id);
                stockVariants.Count.ShouldBe(1);
                stockVariants.First().ProductVariantId.ShouldBe(_testVariant3.Id);
            });
        });
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Stock_And_Related_Data()
    {
        await InTenantAsync(async () =>
        {
            // Arrange
            var createInput = new CreateUpdateStockAggregateDto
            {
                Name = "Stock to Delete",
                Products = new List<StockProductInputDto>
                {
                    new()
                    {
                        ProductId = _testProduct1.Id,
                        Variants = new List<StockVariantInputDto>
                        {
                            new() { ProductVariantId = _testVariant1.Id, Quantity = 2 }
                        }
                    }
                }
            };
            var created = await _stockAppService.CreateAsync(createInput);

            // Act
            await _stockAppService.DeleteAsync(created.Id);

            // Assert
            await WithUnitOfWorkAsync(async () =>
            {
                var stocks = await _stockRepo.GetListAsync(s => s.Id == created.Id);
                stocks.Count.ShouldBe(0);

                var stockProducts = await _stockProductRepo.GetListAsync(sp => sp.StockId == created.Id);
                stockProducts.Count.ShouldBe(0);

                var stockVariants = await _stockProductVariantRepo.GetListAsync();
                stockVariants.Any(sv => stockProducts.Any(sp => sp.Id == sv.StockProductId)).ShouldBeFalse();
            });
        });
    }





    [Fact]
    public async Task CreateAsync_Should_Throw_When_Name_Is_Empty()
    {
        // Arrange
        var input = new CreateUpdateStockAggregateDto
        {
            Name = "",
            Products = new List<StockProductInputDto>()
        };

        // Act , Assert
        var exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _stockAppService.CreateAsync(input);
        });

        exception.Code.ShouldBe("Stock.NameRequired");
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Name_Is_Whitespace()
    {
        // Arrange
        var input = new CreateUpdateStockAggregateDto
        {
            Name = "   ",
            Products = new List<StockProductInputDto>()
        };

        // Act , Assert
        var exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _stockAppService.CreateAsync(input);
        });

        exception.Code.ShouldBe("Stock.NameRequired");
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Quantity_Is_Negative()
    {
        // Arrange
        var input = new CreateUpdateStockAggregateDto
        {
            Name = "Test Stock",
            Products = new List<StockProductInputDto>
            {
                new()
                {
                    ProductId = _testProduct1.Id,
                    Variants = new List<StockVariantInputDto>
                    {
                        new() { ProductVariantId = _testVariant1.Id, Quantity = -1 }
                    }
                }
            }
        };

        // Act , Assert
        var exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _stockAppService.CreateAsync(input);
        });

        exception.Code.ShouldBe("Stock.QuantityNegative");
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Duplicate_Variants_In_Same_Product()
    {
        // Arrange
        var input = new CreateUpdateStockAggregateDto
        {
            Name = "Test Stock",
            Products = new List<StockProductInputDto>
            {
                new()
                {
                    ProductId = _testProduct1.Id,
                    Variants = new List<StockVariantInputDto>
                    {
                        new() { ProductVariantId = _testVariant1.Id, Quantity = 1 },
                        new() { ProductVariantId = _testVariant1.Id, Quantity = 2 } 
                    }
                }
            }
        };

        // Act , Assert
        var exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _stockAppService.CreateAsync(input);
        });

        exception.Code.ShouldBe("Stock.DuplicateVariantInProduct");
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Product_Not_Found()
    {
        // Arrange
        var nonExistentProductId = Guid.NewGuid();
        var input = new CreateUpdateStockAggregateDto
        {
            Name = "Test Stock",
            Products = new List<StockProductInputDto>
            {
                new()
                {
                    ProductId = nonExistentProductId,
                    Variants = new List<StockVariantInputDto>()
                }
            }
        };

        // Act , Assert
        var exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _stockAppService.CreateAsync(input);
        });

        exception.Code.ShouldBe("Stock.ProductNotFound");
        exception.Data["ProductId"].ShouldBe(nonExistentProductId);
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Variant_Not_Found()
    {
        await InTenantAsync(async () =>
        {
            // Arrange
            var nonExistentVariantId = Guid.NewGuid();
            var input = new CreateUpdateStockAggregateDto
            {
                Name = "Test Stock",
                Products = new List<StockProductInputDto>
                {
                    new()
                    {
                        ProductId = _testProduct1.Id,
                        Variants = new List<StockVariantInputDto>
                        {
                            new() { ProductVariantId = nonExistentVariantId, Quantity = 1 }
                        }
                    }
                }
            };

            // Act , Assert
            var exception = await Should.ThrowAsync<BusinessException>(async () =>
            {
                await _stockAppService.CreateAsync(input);
            });

            exception.Code.ShouldBe("Stock.VariantNotFound");
            exception.Data["ProductVariantId"].ShouldBe(nonExistentVariantId);
        });
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Variant_Belongs_To_Different_Product()
    {
        await InTenantAsync(async () =>
        {
            // Arrange 
            var input = new CreateUpdateStockAggregateDto
            {
                Name = "Test Stock",
                Products = new List<StockProductInputDto>
                {
                    new()
                    {
                        ProductId = _testProduct1.Id,
                        Variants = new List<StockVariantInputDto>
                        {
                            new() { ProductVariantId = _testVariant3.Id, Quantity = 1 }
                        }
                    }
                }
            };

            // Act , Assert
            var exception = await Should.ThrowAsync<BusinessException>(async () =>
            {
                await _stockAppService.CreateAsync(input);
            });

            exception.Code.ShouldBe("Stock.ProductVariantMismatch");
            exception.Data["ProductId"].ShouldBe(_testProduct1.Id);
            exception.Data["ProductVariantId"].ShouldBe(_testVariant3.Id);
        });
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Quantity_Exceeds_Available_Stock()
    {
        await InTenantAsync(async () =>
        {
            // Arrange 
            var input = new CreateUpdateStockAggregateDto
            {
                Name = "Test Stock",
                Products = new List<StockProductInputDto>
                {
                    new()
                    {
                        ProductId = _testProduct1.Id,
                        Variants = new List<StockVariantInputDto>
                        {
                            new() { ProductVariantId = _testVariant1.Id, Quantity = 15 } 
                        }
                    }
                }
            };

            // Act , Assert
            var exception = await Should.ThrowAsync<BusinessException>(async () =>
            {
                await _stockAppService.CreateAsync(input);
            });

            exception.Code.ShouldBe("Stock.QuantityExceedsVariantStock");
            exception.Data["ProductId"].ShouldBe(_testProduct1.Id);
            exception.Data["ProductVariantId"].ShouldBe(_testVariant1.Id);
            exception.Data["RequestedQuantity"].ShouldBe(15);
            exception.Data["AvailableStock"].ShouldBe(10);
        });
    }

    [Fact]
    public async Task GetAsync_Should_Throw_When_Stock_Not_Found()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act , Assert
        await Should.ThrowAsync<EntityNotFoundException>(async () =>
        {
            await _stockAppService.GetAsync(nonExistentId);
        });
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_When_Stock_Not_Found()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var input = new CreateUpdateStockAggregateDto
        {
            Name = "Test",
            Products = new List<StockProductInputDto>()
        };

        // Act , Assert
        await Should.ThrowAsync<EntityNotFoundException>(async () =>
        {
            await _stockAppService.UpdateAsync(nonExistentId, input);
        });
    }

    [Fact]
    public async Task DeleteAsync_Should_Throw_When_Stock_Not_Found()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act , Assert
        await Should.ThrowAsync<EntityNotFoundException>(async () =>
        {
            await _stockAppService.DeleteAsync(nonExistentId);
        });
    }




    [Fact]
    public async Task CreateAsync_Should_Handle_Stock_With_No_Products()
    {
        await InTenantAsync(async () =>
        {
            // Arrange
            var input = new CreateUpdateStockAggregateDto
            {
                Name = "Empty Stock",
                Products = new List<StockProductInputDto>()
            };

            // Act
            var result = await _stockAppService.CreateAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("Empty Stock");
            result.Products.Count.ShouldBe(0);
        });
    }

    [Fact]
    public async Task CreateAsync_Should_Handle_Product_With_No_Variants()
    {
        await InTenantAsync(async () =>
        {
            // Arrange
            var input = new CreateUpdateStockAggregateDto
            {
                Name = "Stock with Product No Variants",
                Products = new List<StockProductInputDto>
                {
                    new()
                    {
                        ProductId = _testProduct1.Id,
                        Variants = new List<StockVariantInputDto>()
                    }
                }
            };

            // Act
            var result = await _stockAppService.CreateAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.Products.Count.ShouldBe(1);
            result.Products.First().Variants.Count.ShouldBe(0);
        });
    }

    [Fact]
    public async Task CreateAsync_Should_Handle_Variant_With_Null_ProductVariantId()
    {
        await InTenantAsync(async () =>
        {
            // Arrange
            var input = new CreateUpdateStockAggregateDto
            {
                Name = "Stock with Null Variant",
                Products = new List<StockProductInputDto>
                {
                    new()
                    {
                        ProductId = _testProduct1.Id,
                        Variants = new List<StockVariantInputDto>
                        {
                            new() { ProductVariantId = null, Quantity = 5 }
                        }
                    }
                }
            };

            // Act
            var result = await _stockAppService.CreateAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.Products.First().Variants.Count.ShouldBe(1);
            result.Products.First().Variants.First().ProductVariantId.ShouldBeNull();
            result.Products.First().Variants.First().Quantity.ShouldBe(5);
        });
    }


}
