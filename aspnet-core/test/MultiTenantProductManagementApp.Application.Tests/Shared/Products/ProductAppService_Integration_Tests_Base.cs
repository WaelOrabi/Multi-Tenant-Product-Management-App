using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MultiTenantProductManagementApp.Products;
using MultiTenantProductManagementApp.Products.Dtos;
using Shouldly;
using Volo.Abp.Uow;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Data;
using Xunit;
using Volo.Abp.Modularity;

namespace MultiTenantProductManagementApp.Shared.Products;

public abstract class ProductAppService_Integration_Tests_Base<TStartupModule> : MultiTenantProductManagementAppTestBase<TStartupModule>, IAsyncLifetime
    where TStartupModule : IAbpModule
{
    protected IProductAppService _productAppService = default!;

    protected static readonly Guid _tenantId = Guid.Parse("121ed384-0fbb-4a05-9a88-aaaaaaaaaaaa"); private static bool KeepDb => string.Equals(Environment.GetEnvironmentVariable("KEEP_TEST_DB"), "1", StringComparison.OrdinalIgnoreCase);

    protected async Task InTenantAsync(Func<Task> action)
    {
        var currentTenant = GetRequiredService<ICurrentTenant>();
        using (currentTenant.Change(_tenantId))
        {
            await action();
        }
    }

    public async Task InitializeAsync()
    {
        _productAppService = GetRequiredService<IProductAppService>();
      

        if (!KeepDb)
        {
            await InTenantAsync(async () =>
            {
                await WithUnitOfWorkAsync(async () =>
                {
                    try
                    {
                        // List all products and remove their variants first, then delete the products
                        var listResult = await _productAppService.GetListAsync(new GetProductListInput
                        {
                            MaxResultCount = 1000,
                            SkipCount = 0
                        });

                        foreach (var p in listResult.Items)
                        {
                            var full = await _productAppService.GetAsync(p.Id);
                            foreach (var v in full.Variants.ToList())
                            {
                                await _productAppService.DeleteVariantAsync(p.Id, v.Id);
                            }
                            await _productAppService.DeleteAsync(p.Id);
                        }
                    }
                    catch (Volo.Abp.Validation.AbpValidationException)
                    {
                        // If validation prevents cleanup (e.g., due to paging constraints), skip cleanup and proceed.
                    }
                });
            });
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Create_and_Get_product_with_variants()
    {
        await InTenantAsync(async () =>
        {
            var input = new CreateUpdateProductDto
            {
                Name = "Phone X",
                Description = "Flagship phone",
                BasePrice = 999.99m,
                Category = "Electronics",
                Status = ProductStatus.Active,
                HasVariants = true,
                Variants = new List<CreateUpdateProductVariantDto>
                {
                    new() { Price = 1099.99m, Sku = "PX-BLK-128", Options = new List<ProductVariantOptionDto>{ new(){ Name = "Color", Value = "Black" }, new(){ Name = "Size", Value = "128GB" } } },
                    new() { Price = 1199.99m, Sku = "PX-SLV-256", Options = new List<ProductVariantOptionDto>{ new(){ Name = "Color", Value = "Silver" }, new(){ Name = "Size", Value = "256GB" } } }
                }
            };

            var created = await _productAppService.CreateAsync(input);

            created.ShouldNotBeNull();
            created.Id.ShouldNotBe(Guid.Empty);
            created.Name.ShouldBe("Phone X");
            created.Variants.Count.ShouldBe(2);
            created.Variants.Any(v => v.Sku == "PX-BLK-128").ShouldBeTrue();

            await WithUnitOfWorkAsync(async () =>
            {
                var product = await _productAppService.GetAsync(created.Id);
                product.ShouldNotBeNull();
                product.Variants.Count.ShouldBe(2);
                product.Variants.Any(v => v.Sku == "PX-SLV-256").ShouldBeTrue();
            });
        });
    }

    [Fact]
    public async Task Update_product_replaces_variants_and_updates_fields()
    {
        await InTenantAsync(async () =>
        {
            var created = await _productAppService.CreateAsync(new CreateUpdateProductDto
            {
                Name = "T-Shirt",
                Description = "Basic tee",
                BasePrice = 10m,
                Category = "Apparel",
                Status = ProductStatus.Inactive,
                HasVariants = true,
                Variants = new List<CreateUpdateProductVariantDto>
                {
                    new() { Price = 12m, Sku = "TS-RED-M", Options = new List<ProductVariantOptionDto>{ new(){ Name = "Color", Value = "Red" }, new(){ Name = "Size", Value = "M" } } }
                }
            });

            var updateInput = new CreateUpdateProductDto
            {
                Name = "Premium T-Shirt",
                Description = "Comfy tee",
                BasePrice = 15m,
                Category = "Clothing",
                Status = ProductStatus.Active,
                HasVariants = true,
                Variants = new List<CreateUpdateProductVariantDto>
                {
                    new() { Price = 17m, Sku = "PTS-BLK-L", Options = new List<ProductVariantOptionDto>{ new(){ Name = "Color", Value = "Black" }, new(){ Name = "Size", Value = "L" } } }
                }
            };

            var updated = await _productAppService.UpdateAsync(created.Id, updateInput);

            updated.Name.ShouldBe("Premium T-Shirt");
            updated.Category.ShouldBe("Clothing");
            updated.Status.ShouldBe(ProductStatus.Active);
            updated.Variants.Count.ShouldBe(1);
            updated.Variants[0].Sku.ShouldBe("PTS-BLK-L");

            await WithUnitOfWorkAsync(async () =>
            {
                var product = await _productAppService.GetAsync(created.Id);
                product.Variants.Count.ShouldBe(1);
                product.Variants[0].Sku.ShouldBe("PTS-BLK-L");
            });
        });
    }

    [Fact]
    public async Task Get_non_existing_product_throws_EntityNotFoundException()
    {
        await InTenantAsync(async () =>
        {
            await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(async () =>
            {
                await _productAppService.GetAsync(Guid.NewGuid());
            });
        });
    }

    [Fact]
    public async Task Add_Update_Delete_variant_flow_and_mismatch_failure()
    {
        await InTenantAsync(async () =>
        {
            var product = await _productAppService.CreateAsync(new CreateUpdateProductDto
            {
                Name = "Laptop",
                Description = "Ultrabook",
                BasePrice = 1500m,
                Category = "Electronics",
                Status = ProductStatus.Active,
                HasVariants = false,
                Variants = new List<CreateUpdateProductVariantDto>()
            });

            var vDto = await _productAppService.AddVariantAsync(product.Id, new CreateUpdateProductVariantDto
            {
                Price = 1599m,
                Sku = "LP-GRY-16",
                Options = new List<ProductVariantOptionDto>{ new(){ Name = "Color", Value = "Gray" }, new(){ Name = "Size", Value = "16GB" } }
            });
            vDto.ProductId.ShouldBe(product.Id);

            var updatedVariant = await _productAppService.UpdateVariantAsync(product.Id, vDto.Id, new CreateUpdateProductVariantDto
            {
                Price = 1699m,
                Sku = "LP-GRY-32",
                Options = new List<ProductVariantOptionDto>{ new(){ Name = "Color", Value = "Gray" }, new(){ Name = "Size", Value = "32GB" } }
            });
            updatedVariant.Sku.ShouldBe("LP-GRY-32");

            await WithUnitOfWorkAsync(async () =>
            {
                var updated = await _productAppService.GetAsync(product.Id);
                updated.Variants.Count.ShouldBe(1);
                updated.Variants[0].Sku.ShouldBe("LP-GRY-32");
            });

            await Should.ThrowAsync<Volo.Abp.BusinessException>(async () =>
            {
                await _productAppService.UpdateVariantAsync(Guid.NewGuid(), vDto.Id, new CreateUpdateProductVariantDto
                {
                    Price = 1700m, Sku = "X", Options = new List<ProductVariantOptionDto>{ new(){ Name = "Color", Value = "X" }, new(){ Name = "Size", Value = "X" } }
                });
            });

                await _productAppService.DeleteVariantAsync(product.Id, vDto.Id);

                await WithUnitOfWorkAsync(async () =>
                {
                    var refreshed = await _productAppService.GetAsync(product.Id);
                    refreshed.Variants.Count.ShouldBe(0);
                });
        });
    }

    [Fact]
    public async Task Delete_product_removes_from_database()
    {
        await InTenantAsync(async () =>
        {
            var created = await _productAppService.CreateAsync(new CreateUpdateProductDto
            {
                Name = "Mouse",
                BasePrice = 20m,
                Status = ProductStatus.Inactive,
                HasVariants = false,
            });

                await _productAppService.DeleteAsync(created.Id);

                await WithUnitOfWorkAsync(async () =>
                {
                    await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(async () =>
                    {
                        await _productAppService.GetAsync(created.Id);
                    });
                });
        });
    }

  
}
