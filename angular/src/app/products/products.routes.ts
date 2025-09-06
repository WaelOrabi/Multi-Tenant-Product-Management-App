import { Routes } from '@angular/router';
import { provideRouter } from '@angular/router';

export const productsRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./product-list/products-list.component').then(m => m.ProductsListComponent),
  },
  {
    path: 'create',
    loadComponent: () => import('./product-form/product-form.component').then(m => m.ProductFormComponent),
    data: { mode: 'create' },
  },
  {
    path: ':id',
    loadComponent: () => import('./product-details/product-details.component').then(m => m.ProductDetailsComponent),
  },
  {
    path: ':id/edit',
    loadComponent: () => import('./product-form/product-form.component').then(m => m.ProductFormComponent),
    data: { mode: 'edit' },
  },
];

export default provideRouter(productsRoutes);
