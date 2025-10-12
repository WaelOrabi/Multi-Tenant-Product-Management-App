import { Routes, provideRouter } from '@angular/router';

export const stocksRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./stock-list/stock-list.component').then(m => m.StockListComponent),
  },
  {
    path: 'create',
    loadComponent: () => import('./stock-form/stock-form.component').then(m => m.StockFormComponent),
    data: { mode: 'create', requiredPolicy: 'MultiTenantProductManagementApp.Stocks.Create' },
  },
  {
    path: ':id/edit',
    loadComponent: () => import('./stock-form/stock-form.component').then(m => m.StockFormComponent),
    data: { mode: 'edit', requiredPolicy: 'MultiTenantProductManagementApp.Stocks.Edit' },
  },
];

export default provideRouter(stocksRoutes);
