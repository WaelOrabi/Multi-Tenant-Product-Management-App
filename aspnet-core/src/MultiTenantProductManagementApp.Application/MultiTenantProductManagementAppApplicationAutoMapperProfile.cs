using AutoMapper;
using MultiTenantProductManagementApp.Products;
using MultiTenantProductManagementApp.Products.Dtos;
using MultiTenantProductManagementApp.Stocks.Dtos;
using MultiTenantProductManagementApp.Stocks;

namespace MultiTenantProductManagementApp;

public class MultiTenantProductManagementAppApplicationAutoMapperProfile : Profile
{
    public MultiTenantProductManagementAppApplicationAutoMapperProfile()
    {
    
        CreateMap<Product, ProductDto>();
        CreateMap<ProductVariant, ProductVariantDto>();

        CreateMap<CreateUpdateProductVariantDto, ProductVariant>();
        CreateMap<CreateUpdateProductDto, Product>();

        CreateMap<Stock, StockSummaryDto>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name));
    }
}
