import { mapEnumToOptions } from '@abp/ng.core';

export enum ProductStatus {
  Draft = 0,
  Inactive = 1,
  Active = 2,
}

export const productStatusOptions = mapEnumToOptions(ProductStatus);
