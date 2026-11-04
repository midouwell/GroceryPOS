import { Routes } from '@angular/router';
export const checkoutRoutes: Routes = [{ path: '', loadComponent: () => import('./checkout.page').then(module => module.CheckoutPage) }];
