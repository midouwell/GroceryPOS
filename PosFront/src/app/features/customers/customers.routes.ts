import { Routes } from '@angular/router';
export const customersRoutes: Routes = [{ path: '', loadComponent: () => import('../workspace/workspace.page').then(module => module.WorkspacePage), data: { page: 'customers' } }];
