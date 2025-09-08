import type { EntityDto } from '@abp/ng.core';

export interface CreateUpdateStockAggregateDto {
  name?: string;
  products: StockProductInputDto[];
}

export interface StockDetailDto extends EntityDto<string> {
  name?: string;
  products: StockProductDto[];
}

export interface StockProductDto {
  productId?: string;
  productName?: string;
  variants: StockVariantDto[];
}

export interface StockProductInputDto {
  productId?: string;
  variants: StockVariantInputDto[];
}

export interface StockSummaryDto extends EntityDto<string> {
  name?: string;
}

export interface StockVariantDto {
  productVariantId?: string;
  quantity: number;
  variantSku?: string;
  color?: string;
  size?: string;
}

export interface StockVariantInputDto {
  productVariantId?: string;
  quantity: number;
}
