using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MultiTenantProductManagementApp.EntityFrameworkCore;
using MultiTenantProductManagementApp.Products;
using MultiTenantProductManagementApp.Products.Dtos;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Data;
using Xunit;
using MultiTenantProductManagementApp.Application.Tests;
using MultiTenantProductManagementApp.Testing;

namespace MultiTenantProductManagementApp.Application.Tests.Products;

public class ProductAppService_Integration_Tests : MultiTenantProductManagementAppEntityFrameworkCoreTestBase, IAsyncLifetime
{
    private IProductAppService _productAppService = default!;
    private IRepository<Product, Guid> _productRepo = default!;
    private IRepository<ProductVariant, Guid> _variantRepo = default!;
    private static readonly Guid _tenantId = Guid.NewGuid();

    private async Task InTenantAsync(Func<Task> action)
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
        _productRepo = GetRequiredService<IRepository<Product, Guid>>();
        _variantRepo = GetRequiredService<IRepository<ProductVariant, Guid>>();

        await InTenantAsync(async () =>
        {
            await WithUnitOfWorkAsync(async () =>
            {
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
        });
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [EfOnlyFact]
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
                    new() { Price = 1099.99m, StockQuantity = 5, Sku = "PX-BLK-128", Color = "Black", Size = "128GB" },
                    new() { Price = 1199.99m, StockQuantity = 3, Sku = "PX-SLV-256", Color = "Silver", Size = "256GB" }
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
                var product = await _productRepo.GetAsync(created.Id);
                product.ShouldNotBeNull();

                var variants = await _variantRepo.GetListAsync(v => v.ProductId == product.Id);
                variants.Count.ShouldBe(2);
                variants.Any(v => v.Sku == "PX-SLV-256").ShouldBeTrue();
            });
        });
    }

    [EfOnlyFact]
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
                    new() { Price = 12m, StockQuantity = 10, Sku = "TS-RED-M", Color = "Red", Size = "M" }
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
                    new() { Price = 17m, StockQuantity = 7, Sku = "PTS-BLK-L", Color = "Black", Size = "L" }
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
                var variants = await _variantRepo.GetListAsync(v => v.ProductId == created.Id);
                variants.Count.ShouldBe(1);
                variants[0].Sku.ShouldBe("PTS-BLK-L");
            });
        });
    }

    [EfOnlyFact]
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

    [EfOnlyFact]
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
                StockQuantity = 4,
                Sku = "LP-GRY-16",
                Color = "Gray",
                Size = "16GB"
            });
            vDto.ProductId.ShouldBe(product.Id);

            var updatedVariant = await _productAppService.UpdateVariantAsync(product.Id, vDto.Id, new CreateUpdateProductVariantDto
            {
                Price = 1699m,
                StockQuantity = 2,
                Sku = "LP-GRY-32",
                Color = "Gray",
                Size = "32GB"
            });
            updatedVariant.Sku.ShouldBe("LP-GRY-32");

            await WithUnitOfWorkAsync(async () =>
            {
                var variants = await _variantRepo.GetListAsync(v => v.ProductId == product.Id);
                variants.Count.ShouldBe(1);
                variants[0].Sku.ShouldBe("LP-GRY-32");
            });

            await Should.ThrowAsync<Volo.Abp.BusinessException>(async () =>
            {
                await _productAppService.UpdateVariantAsync(Guid.NewGuid(), vDto.Id, new CreateUpdateProductVariantDto
                {
                    Price = 1700m, StockQuantity = 1, Sku = "X", Color = "X", Size = "X"
                });
            });

            await _productAppService.DeleteVariantAsync(product.Id, vDto.Id);

            await WithUnitOfWorkAsync(async () =>
            {
                var variants = await _variantRepo.GetListAsync(v => v.ProductId == product.Id);
                variants.Count.ShouldBe(0);
            });
        });
    }

    [EfOnlyFact]
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
                var list = await _productRepo.GetListAsync(p => p.Id == created.Id);
                list.Count.ShouldBe(0);
            });
        });
    }
}
