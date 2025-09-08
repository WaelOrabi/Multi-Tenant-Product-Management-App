import type { CreateUpdateStockAggregateDto, StockDetailDto, StockSummaryDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StockAggregateService {
  apiName = 'Default';
  

  create = (input: CreateUpdateStockAggregateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StockDetailDto>({
      method: 'POST',
      url: '/api/app/stock-aggregate',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/stock-aggregate/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StockDetailDto>({
      method: 'GET',
      url: `/api/app/stock-aggregate/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: PagedAndSortedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StockSummaryDto>>({
      method: 'GET',
      url: '/api/app/stock-aggregate',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateStockAggregateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StockDetailDto>({
      method: 'PUT',
      url: `/api/app/stock-aggregate/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
