import type { CreateUpdateProductDto, CreateUpdateProductVariantDto, GetProductListInput, ProductDto, ProductVariantDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  apiName = 'Default';
  

  addVariant = (productId: string, input: CreateUpdateProductVariantDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductVariantDto>({
      method: 'POST',
      url: `/api/app/product/variant/${productId}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateProductDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductDto>({
      method: 'POST',
      url: '/api/app/product',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/product/${id}`,
    },
    { apiName: this.apiName,...config });
  

  deleteVariant = (productId: string, variantId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: '/api/app/product/variant',
      params: { productId, variantId },
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductDto>({
      method: 'GET',
      url: `/api/app/product/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetProductListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProductDto>>({
      method: 'GET',
      url: '/api/app/product',
      params: { filterText: input.filterText, name: input.name, category: input.category, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateProductDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductDto>({
      method: 'PUT',
      url: `/api/app/product/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateVariant = (productId: string, variantId: string, input: CreateUpdateProductVariantDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductVariantDto>({
      method: 'PUT',
      url: '/api/app/product/variant',
      params: { productId, variantId },
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
