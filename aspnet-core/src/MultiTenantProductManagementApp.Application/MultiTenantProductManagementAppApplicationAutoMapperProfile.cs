using AutoMapper;
using MultiTenantProductManagementApp.Products;
using MultiTenantProductManagementApp.Products.Dtos;

namespace MultiTenantProductManagementApp;

public class MultiTenantProductManagementAppApplicationAutoMapperProfile : Profile
{
    public MultiTenantProductManagementAppApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        // Entity -> DTO
        CreateMap<Product, ProductDto>();
        CreateMap<ProductVariant, ProductVariantDto>();

        // DTO -> Entity (for updates if needed)
        CreateMap<CreateUpdateProductVariantDto, ProductVariant>();
        CreateMap<CreateUpdateProductDto, Product>();
    }
}
