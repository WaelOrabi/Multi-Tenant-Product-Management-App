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
   This applies migrations and seeds data (tenants, users, sample products).

3. Start the API Host from `aspnet-core/src/MultiTenantProductManagementApp.HttpApi.Host`:
   ```bash
   dotnet run
   ```
   Default URL (per ABP defaults): `https://localhost:44307` / `http://localhost:22742` (your ports may differ; check console output).

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
   App will open at `http://localhost:4200/`.

## Demo Tenants and Admins
Seeded by `aspnet-core/src/MultiTenantProductManagementApp.Domain/Data/ProductDemoDataSeedContributor.cs`:
- Tenant: `store-one`, Admin: `admin1@demo.com`, Password: `1q2w3E*`
- Tenant: `store-two`, Admin: `admin2@demo.com`, Password: `1q2w3E*`

### Important: Select a Tenant before Login
- Credentials are tenant‑scoped. Use the tenant selector in the UI or add to URL:
  - `http://localhost:4200/?__tenant=store-one`
  - `http://localhost:4200/?__tenant=store-two`
- Logging in at host context (no tenant) will fail with "Invalid username or password".

## Features
- Multi‑tenant separation via ABP `ICurrentTenant`
- Product CRUD with single or multi‑variant support
- Variant attributes, price, and stock
- Search/filter on listing
- Responsive UI (LeptonX Lite)

## Common Tasks
- Create/Edit Products and Variants in Angular UI under `Products`
- Backend entities/services under:
  - `aspnet-core/src/MultiTenantProductManagementApp.Domain/`
  - `aspnet-core/src/MultiTenantProductManagementApp.Application/`

## Configuration
- API base URL is provided by ABP Angular packages. Ensure backend is running and CORS allows `http://localhost:4200`.
- Adjust database connection in `aspnet-core/src/MultiTenantProductManagementApp.HttpApi.Host/appsettings.json` if needed.

## Troubleshooting
- "Invalid username or password": ensure tenant selected (`?__tenant=store-two`) and that seeding ran (check `AbpTenants` and `AbpUsers`).
- Angular not picking global styles: repository uses `angular/src/styles.css` configured in `angular/angular.json`.
- If migrations fail, delete/recreate dev DB, then re‑run DbMigrator.

## Scripts
- Backend migrate/seed: `dotnet run` in `aspnet-core/src/MultiTenantProductManagementApp.DbMigrator`
- Backend run: `dotnet run` in `aspnet-core/src/MultiTenantProductManagementApp.HttpApi.Host`
- Frontend run: `npm run start` in `angular/`


