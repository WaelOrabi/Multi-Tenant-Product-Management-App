# Multi‑Tenant Product Management App

A demo multi‑tenant product management application built with ABP Framework (ASP.NET Core backend) and Angular frontend.



## Repository Structure
- `aspnet-core/` — Backend solution (ABP modules and HttpApi.Host)
- `angular/` — Angular SPA
- `abp_multi_tenant_product_management_task_angular.md` — Task spec and notes

## Quick Start

### 1) Backend: migrate database and seed
1. Open a terminal in `aspnet-core/src/MultiTenantProductManagementApp.DbMigrator`
2. Run:
   ```bash
   dotnet run
   ```
   This applies migrations and seeds data (tenants, users, sample products, role permissions, OpenIddict clients).

3. Start the API Host from `aspnet-core/src/MultiTenantProductManagementApp.HttpApi.Host`:
   ```bash
   dotnet run
   ```
   The API will listen on `https://localhost:44320` (check console output to confirm the port).

### 2) Frontend: run Angular dev server
1. Open a terminal in `angular/`
2. Install packages (first time):
   ```bash
   npm install
   ```
3. Start the dev server:
   ```bash
   npm run start
   ```
   The app opens at `http://localhost:4200/`.

## Demo Tenants and Admins
Seeded by `aspnet-core/src/MultiTenantProductManagementApp.Domain/Data/ProductDemoDataSeedContributor.cs`:
- Tenant: `store-one`, Admin: `admin1@demo.com`, Password: `1q2w3E*`
- Tenant: `store-two`, Admin: `admin2@demo.com`, Password: `1q2w3E*`

### Important: Select a Tenant before Login
- Credentials are tenant‑scoped. Use one of the following methods:
  UI method (recommended): open the top‑right user menu and choose "Switch Tenant" (or the tenant selector), then enter:
     - `store-one`
     - `store-two`


## Features
- Multi‑tenant separation via ABP `ICurrentTenant`
- Products and Stocks modules with permissions
- Product CRUD with variants (attributes, price)
- Responsive UI (LeptonX Lite)
- Identity and Roles management via ABP

## Menus and Permissions
Menus in `angular/src/app/route.provider.ts` are shown only if the logged‑in user has required policies:
- Products: `MultiTenantProductManagementApp.Products`
- Stocks: `MultiTenantProductManagementApp.Stocks`
- Identity (Users): `AbpIdentity.Users`
- Roles: `AbpIdentity.Roles`

DbMigrator seeds these permissions to the `admin` role and assigns the seeded admin users to that role per tenant. After running DbMigrator and logging in as the tenant admin, these menus should appear.

## Configuration
- Backend base URL (issuer) in the Angular environment should match the host: `https://localhost:44320`.
- CORS must allow `http://localhost:4200` (configured by ABP template).
- Database connection can be updated in `aspnet-core/src/MultiTenantProductManagementApp.HttpApi.Host/appsettings.json`.


## Scripts
- Backend migrate/seed: `dotnet run` in `aspnet-core/src/MultiTenantProductManagementApp.DbMigrator`
- Backend run: `dotnet run` in `aspnet-core/src/MultiTenantProductManagementApp.HttpApi.Host`
- Frontend run: `npm run start` in `angular/`

