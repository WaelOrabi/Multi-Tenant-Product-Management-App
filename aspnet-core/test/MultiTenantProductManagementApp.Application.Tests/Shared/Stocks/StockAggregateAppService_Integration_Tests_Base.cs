using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MultiTenantProductManagementApp.Products;
using MultiTenantProductManagementApp.Stocks;
using MultiTenantProductManagementApp.Stocks.Dtos;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Uow;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Xunit;
using Volo.Abp.Modularity;

namespace MultiTenantProductManagementApp.Shared.Stocks;

public abstract class StockAggregateAppService_Integration_Tests_Base<TStartupModule> : MultiTenantProductManagementAppTestBase<TStartupModule>, IAsyncLifetime
    where TStartupModule : IAbpModule
{
    protected IStockAggregateAppService _stockAppService = default!;
    protected IProductAppService _productAppService = default!;

    protected static readonly Guid _tenantId = Guid.Parse("121ed384-0fbb-4a05-9a88-aaaaaaaaaaaa");
    private static bool KeepDb => string.Equals(Environment.GetEnvironmentVariable("KEEP_TEST_DB"), "1", StringComparison.OrdinalIgnoreCase);
    
    protected Product _testProduct1 = default!;
    protected Product _testProduct2 = default!;
    protected ProductVariant _testVariant1 = default!;
    protected ProductVariant _testVariant2 = default!;
    protected ProductVariant _testVariant3 = default!;

    protected async Task InTenantAsync(Func<Task> action)
    {
        var currentTenant = GetRequiredService<ICurrentTenant>();
        using (currentTenant.Change(_tenantId))
        {
            await action();
        }
    }

    private static string UniqueName(string baseName) => $"{baseName}-{Guid.NewGuid().ToString().Substring(0,8)}";

    public async Task InitializeAsync()
    {
        var currentTenant = GetRequiredService<ICurrentTenant>();
        using (currentTenant.Change(_tenantId))
        {
            _stockAppService = GetRequiredService<IStockAggregateAppService>();
            _productAppService = GetRequiredService<MultiTenantProductManagementApp.Products.IProductAppService>();

            if (!KeepDb)
            {
            try
            {
                await CleanupTestDataAsync();
            }
            catch (Volo.Abp.Validation.AbpValidationException)
            {
            }
            }

            try
            {
                await InTenantAsync(async () => await SeedTestDataAsync());
            }
            catch (Volo.Abp.Validation.AbpValidationException)
            {
            }
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task SeedTestDataAsync()
    {
        await InTenantAsync(async () =>
        {

            var phoneInput = new MultiTenantProductManagementApp.Products.Dtos.CreateUpdateProductDto
            {
                Name = "Test Phone",
                Description = "Test smartphone",
                BasePrice = 999.99m,
                Category = "Electronics",
                Status = ProductStatus.Active,
                HasVariants = true,
                Variants = new List<MultiTenantProductManagementApp.Products.Dtos.CreateUpdateProductVariantDto>
                {
                    new() { Price = 1199.99m, StockQuantity = 10, Sku = "PHONE-BLK-128", Color = "Black", Size = "128GB" },
                    new() { Price = 1299.99m, StockQuantity = 5, Sku = "PHONE-SLV-256", Color = "Silver", Size = "256GB" }
                }
            };

            var laptopInput = new MultiTenantProductManagementApp.Products.Dtos.CreateUpdateProductDto
            {
                Name = "Test Laptop",
                Description = "Test laptop computer",
                BasePrice = 1499.99m,
                Category = "Electronics",
                Status = ProductStatus.Active,
                HasVariants = true,
                Variants = new List<MultiTenantProductManagementApp.Products.Dtos.CreateUpdateProductVariantDto>
                {
                    new() { Price = 1699.99m, StockQuantity = 8, Sku = "LAPTOP-GRY-16", Color = "Gray", Size = "16GB" }
                }
            };

            MultiTenantProductManagementApp.Products.Dtos.ProductDto p1;
            MultiTenantProductManagementApp.Products.Dtos.ProductDto p2;

            try
            {
                p1 = await _productAppService.CreateAsync(phoneInput);
            }
            catch (BusinessException ex) when (ex.Code == "MultiTenantProductManagementApp:ProductDuplicateName")
            {
                var existing = await _productAppService.GetListAsync(new MultiTenantProductManagementApp.Products.Dtos.GetProductListInput { Name = phoneInput.Name, SkipCount = 0, MaxResultCount = 1 });
                if (existing.Items.Count == 0) throw; 
                await _productAppService.UpdateAsync(existing.Items[0].Id, phoneInput);
                p1 = await _productAppService.GetAsync(existing.Items[0].Id);
            }

            try
            {
                p2 = await _productAppService.CreateAsync(laptopInput);
            }
            catch (BusinessException ex) when (ex.Code == "MultiTenantProductManagementApp:ProductDuplicateName")
            {
                var existing = await _productAppService.GetListAsync(new MultiTenantProductManagementApp.Products.Dtos.GetProductListInput { Name = laptopInput.Name, SkipCount = 0, MaxResultCount = 1 });
                if (existing.Items.Count == 0) throw;
                await _productAppService.UpdateAsync(existing.Items[0].Id, laptopInput);
                p2 = await _productAppService.GetAsync(existing.Items[0].Id);
            }

            _testProduct1 = new Product(p1.Id, _tenantId, p1.Name, p1.Description, p1.BasePrice, p1.Category, p1.Status, p1.HasVariants);
            _testProduct2 = new Product(p2.Id, _tenantId, p2.Name, p2.Description, p2.BasePrice, p2.Category, p2.Status, p2.HasVariants);

            var v1 = p1.Variants.First(x => x.Sku == "PHONE-BLK-128");
            var v2 = p1.Variants.First(x => x.Sku == "PHONE-SLV-256");
            var v3 = p2.Variants.First(x => x.Sku == "LAPTOP-GRY-16");
            _testVariant1 = new ProductVariant(v1.Id, _tenantId, _testProduct1.Id, v1.Price, v1.StockQuantity, v1.Sku, v1.Color, v1.Size);
            _testVariant2 = new ProductVariant(v2.Id, _tenantId, _testProduct1.Id, v2.Price, v2.StockQuantity, v2.Sku, v2.Color, v2.Size);
            _testVariant3 = new ProductVariant(v3.Id, _tenantId, _testProduct2.Id, v3.Price, v3.StockQuantity, v3.Sku, v3.Color, v3.Size);
        });
    }

    protected async Task CleanupTestDataAsync()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            try
            {
                const int pageSize = 100;
                while (true)
                {
                    var page = await _stockAppService.GetListAsync(new PagedAndSortedResultRequestDto
                    {
                        MaxResultCount = pageSize,
                        SkipCount = 0
                    });
                    if (page.Items.Count == 0) break;
                    foreach (var s in page.Items.ToList())
                    {
                        await _stockAppService.DeleteAsync(s.Id);
                    }
                }

                const int pPageSize = 100;
                while (true)
                {
                    var pPage = await _productAppService.GetListAsync(new MultiTenantProductManagementApp.Products.Dtos.GetProductListInput
                    {
                        MaxResultCount = pPageSize,
                        SkipCount = 0
                    });
                    if (pPage.Items.Count == 0) break;
                    foreach (var p in pPage.Items.ToList())
                    {
                        var full = await _productAppService.GetAsync(p.Id);
                        foreach (var v in full.Variants.ToList())
                        {
                            await _productAppService.DeleteVariantAsync(p.Id, v.Id);
                        }
                        await _productAppService.DeleteAsync(p.Id);
                    }
                }
            }
            catch (Exception)
            {
            }
        });
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Stock_With_Products_And_Variants()
    {
        await InTenantAsync(async () =>
        {
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

            var result = await _stockAppService.CreateAsync(input);

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
                var full = await _stockAppService.GetAsync(result.Id);
                full.Name.ShouldBe("Test Stock 1");
                full.Products.Count.ShouldBe(2);
                full.Products.SelectMany(p => p.Variants).Count().ShouldBe(3);
            });
        });
    }

    [Fact]
    public async Task GetAsync_Should_Return_Complete_Stock_Details()
    {
        await InTenantAsync(async () =>
        {
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

            var result = await _stockAppService.GetAsync(created.Id);

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

            var result = await _stockAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0
            });

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

            updated.Name.ShouldBe("Updated Stock");
            updated.Products.Count.ShouldBe(1);
            updated.Products.First().ProductId.ShouldBe(_testProduct2.Id);
            updated.Products.First().Variants.First().ProductVariantId.ShouldBe(_testVariant3.Id);
            updated.Products.First().Variants.First().Quantity.ShouldBe(7);

            await WithUnitOfWorkAsync(async () =>
            {
                var full = await _stockAppService.GetAsync(created.Id);
                full.Products.Count.ShouldBe(1);
                full.Products.First().ProductId.ShouldBe(_testProduct2.Id);
                full.Products.First().Variants.Count.ShouldBe(1);
                full.Products.First().Variants.First().ProductVariantId.ShouldBe(_testVariant3.Id);
            });
        });
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Stock_And_Related_Data()
    {
        await InTenantAsync(async () =>
        {
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

            await _stockAppService.DeleteAsync(created.Id);

            await WithUnitOfWorkAsync(async () =>
            {
                await Should.ThrowAsync<EntityNotFoundException>(async () =>
                {
                    await _stockAppService.GetAsync(created.Id);
                });
            });
        });
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Name_Is_Empty()
    {
        var input = new CreateUpdateStockAggregateDto
        {
            Name = "",
            Products = new List<StockProductInputDto>()
        };

        var exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _stockAppService.CreateAsync(input);
        });

        exception.Code.ShouldBe("Stock.NameRequired");
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Name_Is_Whitespace()
    {
        var input = new CreateUpdateStockAggregateDto
        {
            Name = "   ",
            Products = new List<StockProductInputDto>()
        };

        var exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _stockAppService.CreateAsync(input);
        });

        exception.Code.ShouldBe("Stock.NameRequired");
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Quantity_Is_Negative()
    {
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

        var exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _stockAppService.CreateAsync(input);
        });

        exception.Code.ShouldBe("Stock.QuantityNegative");
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Duplicate_Variants_In_Same_Product()
    {
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

        var exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _stockAppService.CreateAsync(input);
        });

        exception.Code.ShouldBe("Stock.DuplicateVariantInProduct");
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Product_Not_Found()
    {
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
            var input = new CreateUpdateStockAggregateDto
            {
                Name = UniqueName("Test Stock"),
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
            var input = new CreateUpdateStockAggregateDto
            {
                Name = UniqueName("Test Stock"),
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
        var nonExistentId = Guid.NewGuid();

        await Should.ThrowAsync<EntityNotFoundException>(async () =>
        {
            await _stockAppService.GetAsync(nonExistentId);
        });
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_When_Stock_Not_Found()
    {
        var nonExistentId = Guid.NewGuid();
        var input = new CreateUpdateStockAggregateDto
        {
            Name = "Test",
            Products = new List<StockProductInputDto>()
        };

        await Should.ThrowAsync<EntityNotFoundException>(async () =>
        {
            await _stockAppService.UpdateAsync(nonExistentId, input);
        });
    }

    [Fact]
    public async Task DeleteAsync_Should_Throw_When_Stock_Not_Found()
    {
        var nonExistentId = Guid.NewGuid();

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
            var input = new CreateUpdateStockAggregateDto
            {
                Name = "Empty Stock",
                Products = new List<StockProductInputDto>()
            };

            var result = await _stockAppService.CreateAsync(input);

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

            var result = await _stockAppService.CreateAsync(input);

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

            var result = await _stockAppService.CreateAsync(input);

            result.ShouldNotBeNull();
            result.Products.First().Variants.Count.ShouldBe(1);
            result.Products.First().Variants.First().ProductVariantId.ShouldBeNull();
            result.Products.First().Variants.First().Quantity.ShouldBe(5);
        });
    }
}
