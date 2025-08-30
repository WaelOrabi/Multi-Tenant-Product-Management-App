import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'MultiTenantProductManagementApp',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: 'https://localhost:44320/',
    redirectUri: baseUrl,
    clientId: 'MultiTenantProductManagementApp_App',
    responseType: 'code',
    scope: 'offline_access MultiTenantProductManagementApp',
    requireHttps: true
  },
  apis: {
    default: {
      url: 'https://localhost:44320',
      rootNamespace: 'MultiTenantProductManagementApp',
    },
  },
} as Environment;
