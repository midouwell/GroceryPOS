import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'dashboard', loadChildren: () => import('./features/dashboard/dashboard.routes').then(module => module.dashboardRoutes) },
  { path: 'checkout', loadChildren: () => import('./features/checkout/checkout.routes').then(module => module.checkoutRoutes) },
  { path: 'products', loadChildren: () => import('./features/products/products.routes').then(module => module.productsRoutes) },
  { path: 'inventory', loadChildren: () => import('./features/inventory/inventory.routes').then(module => module.inventoryRoutes) },
  { path: 'sales', loadChildren: () => import('./features/sales/sales.routes').then(module => module.salesRoutes) },
  { path: 'customers', loadChildren: () => import('./features/customers/customers.routes').then(module => module.customersRoutes) },
  { path: 'reports', loadChildren: () => import('./features/reports/reports.routes').then(module => module.reportsRoutes) },
  { path: '**', loadComponent: () => import('./features/not-found/not-found.page').then(module => module.NotFoundPage) },
];
