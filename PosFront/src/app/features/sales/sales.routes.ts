import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
export const salesRoutes: Routes = [{ path: '', canActivate: [authGuard], loadComponent: () => import('../workspace/workspace.page').then(module => module.WorkspacePage), data: { page: 'sales' } }];
