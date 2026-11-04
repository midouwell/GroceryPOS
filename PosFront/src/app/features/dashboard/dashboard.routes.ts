import { Routes } from '@angular/router';
export const dashboardRoutes: Routes = [{ path: '', loadComponent: () => import('../workspace/workspace.page').then(module => module.WorkspacePage), data: { page: 'dashboard' } }];
