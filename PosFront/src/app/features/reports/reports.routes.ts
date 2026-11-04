import { Routes } from '@angular/router';
export const reportsRoutes: Routes = [{ path: '', loadComponent: () => import('../workspace/workspace.page').then(module => module.WorkspacePage), data: { page: 'reports' } }];
