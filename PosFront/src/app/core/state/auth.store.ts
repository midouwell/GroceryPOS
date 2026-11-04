import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { MockUser } from '../models/user.model';
interface AuthState { user: MockUser | null; isAuthenticated: boolean; token: string | null; }

export const AuthStore = signalStore(
  { providedIn: 'root' },
  withState<AuthState>({ user: { id: 'cashier-01', displayName: 'Jordan Miller', role: 'cashier' }, isAuthenticated: true, token: null }),
  withMethods(store => ({
    signIn(user: MockUser) { patchState(store, { user, isAuthenticated: true, token: null }); },
    signOut() { patchState(store, { user: null, isAuthenticated: false, token: null }); },
  })),
);
