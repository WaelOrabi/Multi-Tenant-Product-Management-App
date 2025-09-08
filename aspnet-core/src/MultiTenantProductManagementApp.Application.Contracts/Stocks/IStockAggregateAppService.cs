using System;
using System.Threading.Tasks;
using MultiTenantProductManagementApp.Stocks.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace MultiTenantProductManagementApp.Stocks;

public interface IStockAggregateAppService : IApplicationService
{
    Task<PagedResultDto<StockSummaryDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<StockDetailDto> GetAsync(Guid id);
    Task<StockDetailDto> CreateAsync(CreateUpdateStockAggregateDto input);
    Task<StockDetailDto> UpdateAsync(Guid id, CreateUpdateStockAggregateDto input);
    Task DeleteAsync(Guid id);
}
