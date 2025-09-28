using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;
using MultiTenantProductManagementApp.Products.Dtos;
using MultiTenantProductManagementApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Guids;
using Volo.Abp.DependencyInjection;

namespace MultiTenantProductManagementApp.Products;

[Authorize(MultiTenantProductManagementAppPermissions.Products.Default)]
public class ProductAppService : ApplicationService, IProductAppService
{
    private readonly IRepository<Product, Guid> _productRepo;
    private readonly IRepository<ProductVariant, Guid> _variantRepo;

    public ProductAppService(
        IRepository<Product, Guid> productRepo,
        IRepository<ProductVariant, Guid> variantRepo)
    {
        _productRepo = productRepo;
        _variantRepo = variantRepo;
    }

    private static ProductDto MapProductToDto(Product entity)
    {
        var dto = new ProductDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            BasePrice = entity.BasePrice,
            Category = entity.Category,
            Status = entity.Status,
            HasVariants = entity.HasVariants,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
        if (entity.Variants != null && entity.Variants.Count > 0)
        {
            dto.Variants = entity.Variants.Select(MapVariantToDto).ToList();
        }
        return dto;
    }


    private static ProductVariantDto MapVariantToDto(ProductVariant v)
    {
        return new ProductVariantDto
        {
            Id = v.Id,
            ProductId = v.ProductId,
            Price = v.Price,
            Sku = v.Sku,
            Color = v.Color,
            Size = v.Size
        };
    }

    public virtual async Task<ProductDto> GetAsync(Guid id)
    {
        var entity = await _productRepo.GetAsync(id);
        var variants = await _variantRepo.GetListAsync(v => v.ProductId == id);
        entity.Variants.Clear();
        foreach (var v in variants)
        {
            entity.Variants.Add(v);
        }
        return MapProductToDto(entity);
    }

    public virtual async Task<PagedResultDto<ProductDto>> GetListAsync(GetProductListInput input)
    {
        var queryable = await _productRepo.WithDetailsAsync(x => x.Variants);

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var ft = input.FilterText!.Trim();
            queryable = queryable.Where(x => x.Name.Contains(ft) || (x.Description != null && x.Description.Contains(ft)));
        }
        if (!input.Name.IsNullOrWhiteSpace())
        {
            var name = input.Name!.Trim();
            queryable = queryable.Where(x => x.Name.Contains(name));
        }
        if (!input.Category.IsNullOrWhiteSpace())
        {
            var category = input.Category!.Trim();
            queryable = queryable.Where(x => x.Category != null && x.Category.Contains(category));
        }
        if (input.Status.HasValue)
        {
            queryable = queryable.Where(x => x.Status == input.Status);
        }

        if (!input.Sorting.IsNullOrWhiteSpace())
        {
            if (input.Sorting!.Equals("Name desc", StringComparison.OrdinalIgnoreCase))
                queryable = queryable.OrderByDescending(x => x.Name);
            else if (input.Sorting!.Equals("Category", StringComparison.OrdinalIgnoreCase))
                queryable = queryable.OrderBy(x => x.Category);
            else if (input.Sorting!.Equals("Category desc", StringComparison.OrdinalIgnoreCase))
                queryable = queryable.OrderByDescending(x => x.Category);
            else if (input.Sorting!.Equals("CreationTime", StringComparison.OrdinalIgnoreCase))
                queryable = queryable.OrderBy(x => x.CreationTime);
            else if (input.Sorting!.Equals("CreationTime desc", StringComparison.OrdinalIgnoreCase))
                queryable = queryable.OrderByDescending(x => x.CreationTime);
            else
                queryable = queryable.OrderBy(x => x.Name);
        }
        else
        {
            queryable = queryable.OrderByDescending(x => x.CreationTime);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var items = await AsyncExecuter.ToListAsync(queryable.Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = items.Select(MapProductToDto).ToList();
        return new PagedResultDto<ProductDto>(totalCount, dtos);
    }

    [Authorize(MultiTenantProductManagementAppPermissions.Products.Create)]
    public virtual async Task<ProductDto> CreateAsync(CreateUpdateProductDto input)
    {
        var createQueryable = await _productRepo.GetQueryableAsync();
        var exists = await AsyncExecuter.AnyAsync(
            createQueryable.Where(x => x.TenantId == CurrentTenant.Id && !x.IsDeleted && x.Name == input.Name)
        );
        if (exists)
            throw new BusinessException("MultiTenantProductManagementApp:ProductDuplicateName").WithData("Name", input.Name);

        var product = new Product(
            LazyServiceProvider.LazyGetRequiredService<IGuidGenerator>().Create(),
            CurrentTenant.Id,
            input.Name,
            input.Description,
            input.BasePrice,
            input.Category,
            input.Status,
            input.HasVariants
        );

        await _productRepo.InsertAsync(product, autoSave: true);

        if (input.Variants != null && input.Variants.Count > 0)
        {
            product.EnableVariants();
            foreach (var v in input.Variants)
            {
                var variant = new ProductVariant(
                    LazyServiceProvider.LazyGetRequiredService<IGuidGenerator>().Create(),
                    product.TenantId,
                    product.Id,
                    v.Price,
                    v.Sku,
                    v.Color,
                    v.Size
                );
                await _variantRepo.InsertAsync(variant, autoSave: true);
            }
        }

        var variants = await _variantRepo.GetListAsync(v => v.ProductId == product.Id);
        product.Variants.Clear();
        foreach (var v in variants)
        {
            product.Variants.Add(v);
        }
        return MapProductToDto(product);
    }

    [Authorize(MultiTenantProductManagementAppPermissions.Products.Edit)]
    public virtual async Task<ProductDto> UpdateAsync(Guid id, CreateUpdateProductDto input)
    {
        var entity = await _productRepo.GetAsync(id);

        var updateQueryable = await _productRepo.GetQueryableAsync();
        var existsWithName = await AsyncExecuter.AnyAsync(
            updateQueryable.Where(x => x.TenantId == CurrentTenant.Id && !x.IsDeleted && x.Name == input.Name && x.Id != id)
        );
        if (existsWithName)
            throw new BusinessException("MultiTenantProductManagementApp:ProductDuplicateName").WithData("Name", input.Name);

        entity.SetName(input.Name);
        entity.SetDescription(input.Description);
        entity.SetBasePrice(input.BasePrice);
        entity.SetCategory(input.Category);
        entity.SetStatus(input.Status);
        if (input.HasVariants) entity.EnableVariants(); else entity.DisableVariants();

        try
        {
            await _productRepo.UpdateAsync(entity, autoSave: true);
        }
        catch (Volo.Abp.Data.AbpDbConcurrencyException)
        {
            var fresh = await _productRepo.GetAsync(id);
            fresh.SetName(input.Name);
            fresh.SetDescription(input.Description);
            fresh.SetBasePrice(input.BasePrice);
            fresh.SetCategory(input.Category);
            fresh.SetStatus(input.Status);
            if (input.HasVariants) fresh.EnableVariants(); else fresh.DisableVariants();
            entity = fresh;
            await _productRepo.UpdateAsync(entity, autoSave: true);
        }

        await _variantRepo.DeleteAsync(v => v.ProductId == entity.Id);
        if (input.Variants != null)
        {
            foreach (var v in input.Variants)
            {
                var variant = new ProductVariant(
                    LazyServiceProvider.LazyGetRequiredService<IGuidGenerator>().Create(),
                    entity.TenantId,
                    entity.Id,
                    v.Price,
                    v.Sku,
                    v.Color,
                    v.Size
                );
                await _variantRepo.InsertAsync(variant, autoSave: true);
            }
        }

        var variants = await _variantRepo.GetListAsync(v => v.ProductId == entity.Id);
        entity.Variants.Clear();
        foreach (var v in variants)
        {
            entity.Variants.Add(v);
        }
        return MapProductToDto(entity);
    }

    [Authorize(MultiTenantProductManagementAppPermissions.Products.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _productRepo.DeleteAsync(id);
    }

    [Authorize(MultiTenantProductManagementAppPermissions.Products.Edit)]
    public virtual async Task<ProductVariantDto> AddVariantAsync(Guid productId, CreateUpdateProductVariantDto input)
    {
        var product = await _productRepo.GetAsync(productId);
        var variant = new ProductVariant(
            LazyServiceProvider.LazyGetRequiredService<IGuidGenerator>().Create(),
            CurrentTenant.Id,
            product.Id,
            input.Price,
            input.Sku,
            input.Color,
            input.Size
        );
        await _variantRepo.InsertAsync(variant, autoSave: true);
        return MapVariantToDto(variant);
    }

    [Authorize(MultiTenantProductManagementAppPermissions.Products.Edit)]
    public virtual async Task<ProductVariantDto> UpdateVariantAsync(Guid productId, Guid variantId, CreateUpdateProductVariantDto input)
    {
        var variant = await _variantRepo.FindAsync(variantId);
        if (variant == null)
        {
            throw new BusinessException("ProductVariant.ProductMismatch").WithData("ProductId", productId).WithData("VariantId", variantId);
        }
        if (variant.ProductId != productId)
        {
            throw new BusinessException("ProductVariant.ProductMismatch").WithData("ProductId", productId).WithData("VariantId", variantId);
        }
        variant.SetSku(input.Sku);
        variant.SetColor(input.Color);
        variant.SetSize(input.Size);
        variant.SetPrice(input.Price);
        await _variantRepo.UpdateAsync(variant, autoSave: true);
        return MapVariantToDto(variant);
    }

    [Authorize(MultiTenantProductManagementAppPermissions.Products.Delete)]
    public virtual async Task DeleteVariantAsync(Guid productId, Guid variantId)
    {
        var variant = await _variantRepo.GetAsync(variantId);
        if (variant.ProductId != productId)
        {
            throw new BusinessException("ProductVariant.ProductMismatch").WithData("ProductId", productId).WithData("VariantId", variantId);
        }
        await _variantRepo.DeleteAsync(variant);
    }
}
