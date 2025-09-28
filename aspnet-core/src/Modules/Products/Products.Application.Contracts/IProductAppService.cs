using System;
using System.Threading.Tasks;
using MultiTenantProductManagementApp.Products.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace MultiTenantProductManagementApp.Products;

public interface IProductAppService : IApplicationService
{
    Task<ProductDto> GetAsync(Guid id);
    Task<PagedResultDto<ProductDto>> GetListAsync(GetProductListInput input);
    Task<ProductDto> CreateAsync(CreateUpdateProductDto input);
    Task<ProductDto> UpdateAsync(Guid id, CreateUpdateProductDto input);
    Task DeleteAsync(Guid id);

    Task<ProductVariantDto> AddVariantAsync(Guid productId, CreateUpdateProductVariantDto input);
    Task<ProductVariantDto> UpdateVariantAsync(Guid productId, Guid variantId, CreateUpdateProductVariantDto input);
    Task DeleteVariantAsync(Guid productId, Guid variantId);
}
