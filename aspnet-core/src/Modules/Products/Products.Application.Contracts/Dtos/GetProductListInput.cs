using MultiTenantProductManagementApp.Products;
using Volo.Abp.Application.Dtos;

namespace MultiTenantProductManagementApp.Products.Dtos;

public class GetProductListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; } 
    public string? Name { get; set; }
    public string? Category { get; set; }
    public ProductStatus? Status { get; set; }

}
