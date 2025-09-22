using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using MultiTenantProductManagementApp.Permissions;
using MultiTenantProductManagementApp.Products;
using MultiTenantProductManagementApp.Stocks.Dtos;
using Volo.Abp.Guids;
using Volo.Abp.DependencyInjection;

namespace MultiTenantProductManagementApp.Stocks;

[Authorize(MultiTenantProductManagementAppPermissions.Stocks.Default)]
public class StockAggregateAppService : ApplicationService, IStockAggregateAppService
{
    private readonly IRepository<Stock, Guid> _stockRepo;
    private readonly IRepository<StockProduct, Guid> _stockProductRepo;
    private readonly IRepository<StockProductVariant, Guid> _stockProductVariantRepo;
    private readonly IRepository<Product, Guid> _productRepo;
    private readonly IRepository<ProductVariant, Guid> _variantRepo;

    public StockAggregateAppService(
        IRepository<Stock, Guid> stockRepo,
        IRepository<StockProduct, Guid> stockProductRepo,
        IRepository<StockProductVariant, Guid> stockProductVariantRepo,
        IRepository<Product, Guid> productRepo,
        IRepository<ProductVariant, Guid> variantRepo)
    {
        _stockRepo = stockRepo;
        _stockProductRepo = stockProductRepo;
        _stockProductVariantRepo = stockProductVariantRepo;
        _productRepo = productRepo;
        _variantRepo = variantRepo;
    }

    public virtual async Task<PagedResultDto<StockSummaryDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _stockRepo.GetQueryableAsync();
        queryable = queryable.OrderByDescending(x => x.CreationTime);
        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var items = await AsyncExecuter.ToListAsync(queryable.Skip(input.SkipCount).Take(input.MaxResultCount));
        var dtos = items.Select(s => new StockSummaryDto { Id = s.Id, Name = s.Name }).ToList();
        return new PagedResultDto<StockSummaryDto>(totalCount, dtos);
    }

    public virtual async Task<StockDetailDto> GetAsync(Guid id)
    {
        var stock = await _stockRepo.GetAsync(id);
        var detail = new StockDetailDto { Id = stock.Id, Name = stock.Name };

        var products = await _stockProductRepo.GetListAsync(x => x.StockId == stock.Id);
        if (products.Count == 0)
            return detail;

        var productIds = products.Select(p => p.ProductId).Distinct().ToList();
        var productEntities = await _productRepo.GetListAsync(p => productIds.Contains(p.Id));
        var productNameMap = productEntities.ToDictionary(p => p.Id, p => p.Name);

        foreach (var p in products)
        {
            var productDto = new StockProductDto
            {
                ProductId = p.ProductId,
                ProductName = productNameMap.TryGetValue(p.ProductId, out var pn) ? pn : null
            };

            var variants = await _stockProductVariantRepo.GetListAsync(v => v.StockProductId == p.Id);
            if (variants.Count > 0)
            {
                var variantIds = variants.Where(v => v.ProductVariantId.HasValue).Select(v => v.ProductVariantId!.Value).Distinct().ToList();
                var variantEntities = variantIds.Count > 0 ? await _variantRepo.GetListAsync(v => variantIds.Contains(v.Id)) : new List<ProductVariant>();
                var variantMap = variantEntities.ToDictionary(v => v.Id, v => v);

                foreach (var v in variants)
                {
                    var line = new StockVariantDto
                    {
                        ProductVariantId = v.ProductVariantId,
                        Quantity = v.Quantity
                    };
                    if (v.ProductVariantId.HasValue && variantMap.TryGetValue(v.ProductVariantId.Value, out var ve))
                    {
                        line.VariantSku = ve.Sku;
                        line.Color = ve.Color;
                        line.Size = ve.Size;
                    }
                    productDto.Variants.Add(line);
                }
            }

            detail.Products.Add(productDto);
        }

        return detail;
    }

    [Authorize(MultiTenantProductManagementAppPermissions.Stocks.Create)]
    public virtual async Task<StockDetailDto> CreateAsync(CreateUpdateStockAggregateDto input)
    {
        ValidateInput(input);
        var createQueryable = await _stockRepo.GetQueryableAsync();
        var exists = await AsyncExecuter.AnyAsync(
            createQueryable.Where(x => x.TenantId == CurrentTenant.Id && !x.IsDeleted && x.Name == input.Name)
        );
        if (exists)
            throw new BusinessException("MultiTenantProductManagementApp:StockDuplicateName").WithData("Name", input.Name);

        var stock = new Stock(LazyServiceProvider.LazyGetRequiredService<IGuidGenerator>().Create(), CurrentTenant.Id, input.Name);
        await _stockRepo.InsertAsync(stock, autoSave: true);

        await UpsertChildrenAsync(stock, input);

        return await GetAsync(stock.Id);
    }

    [Authorize(MultiTenantProductManagementAppPermissions.Stocks.Edit)]
    public virtual async Task<StockDetailDto> UpdateAsync(Guid id, CreateUpdateStockAggregateDto input)
    {
        ValidateInput(input);
        var updateQueryable = await _stockRepo.GetQueryableAsync();
        var existsWithName = await AsyncExecuter.AnyAsync(
            updateQueryable.Where(x => x.TenantId == CurrentTenant.Id && !x.IsDeleted && x.Name == input.Name && x.Id != id)
        );
        if (existsWithName)
            throw new BusinessException("MultiTenantProductManagementApp:StockDuplicateName").WithData("Name", input.Name);

        var stock = await _stockRepo.GetAsync(id);
        stock.SetName(input.Name);
        await _stockRepo.UpdateAsync(stock, autoSave: true);

        var existingProducts = await _stockProductRepo.GetListAsync(x => x.StockId == stock.Id);
        foreach (var sp in existingProducts)
        {
            await _stockProductVariantRepo.DeleteAsync(v => v.StockProductId == sp.Id);
        }
        await _stockProductRepo.DeleteAsync(x => x.StockId == stock.Id);

        await UpsertChildrenAsync(stock, input);

        return await GetAsync(stock.Id);
    }

    [Authorize(MultiTenantProductManagementAppPermissions.Stocks.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var stock = await _stockRepo.GetAsync(id);
        var products = await _stockProductRepo.GetListAsync(x => x.StockId == stock.Id);
        foreach (var sp in products)
        {
            await _stockProductVariantRepo.DeleteAsync(v => v.StockProductId == sp.Id);
        }
        await _stockProductRepo.DeleteAsync(x => x.StockId == stock.Id);
        await _stockRepo.DeleteAsync(id);
    }

    private void ValidateInput(CreateUpdateStockAggregateDto input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new BusinessException("Stock.NameRequired");

        foreach (var p in input.Products)
        {
            var set = new HashSet<Guid?>(new NullableGuidComparer());
            foreach (var v in p.Variants)
            {
                if (v.Quantity < 0) throw new BusinessException("Stock.QuantityNegative");
                if (!set.Add(v.ProductVariantId)) throw new BusinessException("Stock.DuplicateVariantInProduct");
            }
        }
    }

    private async Task UpsertChildrenAsync(Stock stock, CreateUpdateStockAggregateDto input)
    {
        var productIds = input.Products.Select(p => p.ProductId).Distinct().ToList();
        var products = await _productRepo.GetListAsync(p => productIds.Contains(p.Id));
        var productMap = products.ToDictionary(p => p.Id, p => p);

        foreach (var p in input.Products)
        {
            if (!productMap.ContainsKey(p.ProductId))
                throw new BusinessException("Stock.ProductNotFound").WithData("ProductId", p.ProductId);

            var sp = new StockProduct(LazyServiceProvider.LazyGetRequiredService<IGuidGenerator>().Create(), CurrentTenant.Id, stock.Id, p.ProductId);
            await _stockProductRepo.InsertAsync(sp, autoSave: true);

            var variantIds = p.Variants.Where(v => v.ProductVariantId.HasValue).Select(v => v.ProductVariantId!.Value).Distinct().ToList();
            var variants = variantIds.Count > 0 ? await _variantRepo.GetListAsync(v => variantIds.Contains(v.Id)) : new List<ProductVariant>();
            var variantMap = variants.ToDictionary(v => v.Id, v => v);

            foreach (var v in p.Variants)
            {
                if (v.ProductVariantId.HasValue)
                {
                    if (!variantMap.TryGetValue(v.ProductVariantId.Value, out var ve))
                        throw new BusinessException("Stock.VariantNotFound").WithData("ProductVariantId", v.ProductVariantId);
                    if (ve.ProductId != p.ProductId)
                        throw new BusinessException("Stock.ProductVariantMismatch").WithData("ProductId", p.ProductId).WithData("ProductVariantId", v.ProductVariantId);
                    if (v.Quantity > ve.StockQuantity)
                        throw new BusinessException("Stock.QuantityExceedsVariantStock")
                            .WithData("ProductId", p.ProductId)
                            .WithData("ProductVariantId", v.ProductVariantId)
                            .WithData("RequestedQuantity", v.Quantity)
                            .WithData("AvailableStock", ve.StockQuantity);
                }
                var line = new StockProductVariant(LazyServiceProvider.LazyGetRequiredService<IGuidGenerator>().Create(), CurrentTenant.Id, sp.Id, v.ProductVariantId, v.Quantity);
                await _stockProductVariantRepo.InsertAsync(line, autoSave: true);
            }
        }
    }

    private sealed class NullableGuidComparer : IEqualityComparer<Guid?>
    {
        public bool Equals(Guid? x, Guid? y) => Nullable.Equals(x, y);
        public int GetHashCode(Guid? obj) => obj.HasValue ? obj.Value.GetHashCode() : 0;
    }
}
