using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace MultiTenantProductManagementApp.Stocks.Dtos;

public class StockSummaryDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
}

public class StockDetailDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public List<StockProductDto> Products { get; set; } = new();
}

public class StockProductDto
{
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public List<StockVariantDto> Variants { get; set; } = new();
}

public class StockVariantDto
{
    public Guid? ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public string? VariantSku { get; set; }
}

public class CreateUpdateStockAggregateDto
{
    public string Name { get; set; } = string.Empty;
    public List<StockProductInputDto> Products { get; set; } = new();
}

public class StockProductInputDto
{
    public Guid ProductId { get; set; }
    public List<StockVariantInputDto> Variants { get; set; } = new();
}

public class StockVariantInputDto
{
    public Guid? ProductVariantId { get; set; }
    public int Quantity { get; set; }
}
