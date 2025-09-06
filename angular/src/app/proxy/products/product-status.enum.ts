import { mapEnumToOptions } from '@abp/ng.core';

export enum ProductStatus {
  Inactive = 0,
  Active = 1,
}

export const productStatusOptions = mapEnumToOptions(ProductStatus);
