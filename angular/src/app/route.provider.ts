import { RoutesService, eLayoutType } from '@abp/ng.core';
import { APP_INITIALIZER } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  { provide: APP_INITIALIZER, useFactory: configureRoutes, deps: [RoutesService], multi: true },
];

function configureRoutes(routesService: RoutesService) {
  return () => {
    routesService.add([
      {
        path: '/',
        name: '::Menu:Home',
        iconClass: 'fas fa-home',
        order: 1,
        layout: eLayoutType.application,
      },
      {
        path: '/products',
        name: 'Products',
        iconClass: 'fa fa-box',
        order: 2,
        layout: eLayoutType.application,
        requiredPolicy: 'MultiTenantProductManagementApp.Products',
      },
      {
        path: '/identity/users',
        name: 'Identity',
        iconClass: 'fa fa-users',
        order: 2,
        layout: eLayoutType.application,
        requiredPolicy: 'AbpIdentity.Users',
      },
      {
        path: '/identity/roles',
        name: 'Roles',
        iconClass: 'fa fa-user-secret',
        order: 2,
        layout: eLayoutType.application,
        requiredPolicy: 'AbpIdentity.Roles',
      }
    ]);
  };
}
