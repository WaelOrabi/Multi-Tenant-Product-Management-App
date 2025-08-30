import { Routes } from '@angular/router';
import { provideRouter } from '@angular/router';

export const productsRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./products-list.component').then(m => m.ProductsListComponent),
  },
  {
    path: 'create',
    loadComponent: () => import('./product-form.component').then(m => m.ProductFormComponent),
    data: { mode: 'create' },
  },
  {
    path: ':id/edit',
    loadComponent: () => import('./product-form.component').then(m => m.ProductFormComponent),
    data: { mode: 'edit' },
  },
];

export default provideRouter(productsRoutes);
