import { Routes } from '@angular/router';
import { featureCanMatch } from './shared/feature.guard';

export const appRoutes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadChildren: () => import('./home/home.routes').then(m => m.homeRoutes),
  },
  {
    path: 'products',
    loadChildren: () => import('./products/products.routes').then(m => m.productsRoutes),
  },
  {
    path: 'stocks',
    loadChildren: () => import('./stocks/stocks.routes').then(m => m.stocksRoutes),
    canMatch: [featureCanMatch],
    data: { requiredFeatures: ['MultiTenantProductManagementApp.Stock'] },
  },
  
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(m => m.createRoutes()),
  },
  {
    path: 'identity',
    loadChildren: () => import('@abp/ng.identity').then(m => m.createRoutes()),
  },
  {
    path: 'tenant-management',
    loadChildren: () =>
      import('@abp/ng.tenant-management').then(m => m.createRoutes()),
  },
   {
    path: 'feature-management',
    loadChildren: () =>
      import('@abp/ng.feature-management').then(m =>[
               {
          path: '',
          component: m.FeatureManagementComponent,
        },
      ]),
   },
  {
    path: 'setting-management',
    loadChildren: () =>
      import('@abp/ng.setting-management').then(m => m.createRoutes()),
  },
];
