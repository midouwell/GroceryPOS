import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { Sale, seedSales } from '../../../core/models/pos.models';

interface SalesState { sales: Sale[]; }
export const SalesStore = signalStore(
  { providedIn: 'root' },
  withState<SalesState>({ sales: seedSales }),
  withMethods(store => ({
    recordSale(sale: Sale) {
      patchState(store, state => ({ sales: [sale, ...state.sales] }));
    },
  })),
);
