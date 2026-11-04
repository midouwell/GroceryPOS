import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { UserRole } from '../models/user.model';
import { AuthStore } from '../state/auth.store';

export const roleGuard: CanActivateFn = (route) => {
  const auth = inject(AuthStore);
  const allowed = route.data['roles'] as UserRole[] | undefined;
  if (!auth.isAuthenticated()) return inject(Router).createUrlTree(['/checkout']);
  return !allowed?.length || allowed.includes(auth.user()?.role ?? 'cashier')
    ? true
    : inject(Router).createUrlTree(['/dashboard']);
};
