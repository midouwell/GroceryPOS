export type UserRole = 'cashier' | 'manager' | 'admin';
export interface MockUser {
  id: string;
  displayName: string;
  role: UserRole;
}
