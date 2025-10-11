using System;
using Volo.Abp.Application.Dtos;

namespace MultiTenantProductManagementApp.Products.Dtos
{
    public class ProductLookupDto : EntityDto<Guid>
    {
        public required string Name { get; set; }
    }
}
