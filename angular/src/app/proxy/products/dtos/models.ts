import type { ProductStatus } from '../product-status.enum';
import type { AuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateProductDto {
  name: string;
  description?: string;
  basePrice?: number;
  category?: string;
  status?: ProductStatus;
  hasVariants: boolean;
  variants: CreateUpdateProductVariantDto[];
}

export interface CreateUpdateProductVariantDto {
  sku?: string;
  attributesJson?: string;
  price: number;
  stockQuantity: number;
}

export interface GetProductListInput extends PagedAndSortedResultRequestDto {
  filterText?: string;
  name?: string;
  category?: string;
  status?: ProductStatus;
}

export interface ProductDto extends AuditedEntityDto<string> {
  name?: string;
  description?: string;
  basePrice?: number;
  category?: string;
  status?: ProductStatus;
  hasVariants: boolean;
  variants: ProductVariantDto[];
}

export interface ProductVariantDto extends AuditedEntityDto<string> {
  productId?: string;
  sku?: string;
  attributesJson?: string;
  price: number;
  stockQuantity: number;
}
