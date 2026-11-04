import { Injectable, inject } from '@angular/core';
import { MockUser } from '../models/user.model';
import { AuthStore } from '../state/auth.store';

export interface AuthOperations {
  signIn(user: MockUser): void;
  signOut(): void;
}

/** Local demonstration identity only; it provides no server authentication. */
@Injectable({ providedIn: 'root' })
export class AuthService implements AuthOperations {
  private readonly auth = inject(AuthStore);
  signIn(user: MockUser): void { this.auth.signIn(user); }
  signOut(): void { this.auth.signOut(); }
}
