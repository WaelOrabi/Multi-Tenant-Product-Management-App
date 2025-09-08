import { Routes, provideRouter } from '@angular/router';

export const stocksRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./stock-list/stock-list.component').then(m => m.StockListComponent),
  },
  {
    path: 'create',
    loadComponent: () => import('./stock-form/stock-form.component').then(m => m.StockFormComponent),
    data: { mode: 'create' },
  },
  {
    path: ':id/edit',
    loadComponent: () => import('./stock-form/stock-form.component').then(m => m.StockFormComponent),
    data: { mode: 'edit' },
  },
];

export default provideRouter(stocksRoutes);
