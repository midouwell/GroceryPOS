import { Routes } from '@angular/router';
import { roleGuard } from '../../core/guards/role.guard';
export const inventoryRoutes: Routes = [{ path: '', canActivate: [roleGuard], loadComponent: () => import('../workspace/workspace.page').then(module => module.WorkspacePage), data: { page: 'inventory', roles: ['cashier', 'manager', 'admin'] } }];
