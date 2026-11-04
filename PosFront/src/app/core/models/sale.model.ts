import { CartLine } from './cart-item.model';
export type PaymentMethod = 'cash' | 'card';
export interface Sale {
  id: string;
  saleNumber: string;
  date: string;
  lines: CartLine[];
  subtotalCents: number;
  taxCents: number;
  totalCents: number;
  paymentMethod: PaymentMethod;
  status: 'completed';
}
