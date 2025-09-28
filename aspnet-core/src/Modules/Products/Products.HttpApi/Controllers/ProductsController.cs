using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using MultiTenantProductManagementApp.Products;
using MultiTenantProductManagementApp.Products.Dtos;

namespace MultiTenantProductManagementApp.Products.Controllers;

[Route("api/products")]
public class ProductsController : AbpController
{
    private readonly IProductAppService _service;

    public ProductsController(IProductAppService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public Task<ProductDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpGet]
    public Task<PagedResultDto<ProductDto>> GetListAsync([FromQuery] GetProductListInput input)
        => _service.GetListAsync(input);

    [HttpPost]
    public Task<ProductDto> CreateAsync([FromBody] CreateUpdateProductDto input)
        => _service.CreateAsync(input);

    [HttpPut("{id}")]
    public Task<ProductDto> UpdateAsync(Guid id, [FromBody] CreateUpdateProductDto input)
        => _service.UpdateAsync(id, input);

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id) => _service.DeleteAsync(id);

    [HttpPost("{productId}/variants")]
    public Task<ProductVariantDto> AddVariantAsync(Guid productId, [FromBody] CreateUpdateProductVariantDto input)
        => _service.AddVariantAsync(productId, input);

    [HttpPut("{productId}/variants/{variantId}")]
    public Task<ProductVariantDto> UpdateVariantAsync(Guid productId, Guid variantId, [FromBody] CreateUpdateProductVariantDto input)
        => _service.UpdateVariantAsync(productId, variantId, input);

    [HttpDelete("{productId}/variants/{variantId}")]
    public Task DeleteVariantAsync(Guid productId, Guid variantId)
        => _service.DeleteVariantAsync(productId, variantId);
}
