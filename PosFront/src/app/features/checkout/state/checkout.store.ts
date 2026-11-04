import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { catchError, EMPTY, exhaustMap, pipe, tap, timer } from 'rxjs';
import { MockCheckoutService } from '../../../core/services/mock-checkout.service';
import { Sale } from '../../../core/models/pos.models';
import { CartStore } from './cart.store';
import { SalesStore } from '../../sales/state/sales.store';

export type CheckoutStatus = 'idle' | 'processing' | 'succeeded' | 'failed';
interface CheckoutState {
  status: CheckoutStatus;
  error: string | null;
  cashTenderedCents: number;
  cashDueCents: number;
  receipt: Sale | null;
}
const initialState: CheckoutState = { status: 'idle', error: null, cashTenderedCents: 0, cashDueCents: 0, receipt: null };

export const CheckoutStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed(store => {
    const cart = inject(CartStore);
    return {
      changeDueCents: computed(() => Math.max(0, store.cashTenderedCents() - store.cashDueCents())),
      canSubmit: computed(() => !cart.isEmpty() && store.status() !== 'processing'),
    };
  }),
  withMethods(store => {
    const cart = inject(CartStore);
    const sales = inject(SalesStore);
    const checkout = inject(MockCheckoutService);
    return {
      setCashTendered(cashTenderedCents: number) {
        patchState(store, { cashTenderedCents: Math.max(0, cashTenderedCents) });
      },
      reset() {
        patchState(store, initialState);
      },
      submitCash: rxMethod<number>(pipe(
        exhaustMap(cashTenderedCents => {
          if (store.status() === 'processing') return EMPTY;
          patchState(store, { status: 'processing', error: null, cashTenderedCents, cashDueCents: cart.totalCents() });
          return timer(250).pipe(
            tap(() => {
              const sale = checkout.recordCashSale(cart.lines(), cart.subtotalCents(), cart.taxCents(), cashTenderedCents);
              sales.recordSale(sale);
              cart.clear();
              patchState(store, { status: 'succeeded', error: null, receipt: sale });
            }),
            catchError((error: unknown) => {
              patchState(store, { status: 'failed', error: error instanceof Error ? error.message : 'Payment could not be completed.' });
              return EMPTY;
            }),
          );
        }),
      )),
    };
  }),
);
