import { computed } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { CartLine, Product } from '../../../core/models/pos.models';

interface CartState { lines: CartLine[]; }
const initialState: CartState = { lines: [] };

const snapshotProduct = (product: Product): CartLine => ({
  productId: product.id,
  name: product.name,
  sku: product.sku,
  unit: product.unit,
  unitPriceCents: Math.round(product.price * 100),
  taxRate: product.taxRate,
  quantity: 1,
});

export const CartStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed(({ lines }) => ({
    itemCount: computed(() => lines().reduce((count, item) => count + item.quantity, 0)),
    subtotalCents: computed(() => lines().reduce((sum, item) => sum + item.unitPriceCents * item.quantity, 0)),
    taxCents: computed(() => lines().reduce((sum, item) => sum + Math.round(item.unitPriceCents * item.quantity * item.taxRate), 0)),
    isEmpty: computed(() => lines().length === 0),
  })),
  withComputed(store => ({
    totalCents: computed(() => store.subtotalCents() + store.taxCents()),
    checkoutAvailable: computed(() => !store.isEmpty()),
  })),
  withMethods(store => ({
    addProduct(product: Product) {
      if (!product.isActive) return;
      patchState(store, state => ({
        lines: state.lines.some(line => line.productId === product.id)
          ? state.lines.map(line => line.productId === product.id ? { ...line, quantity: line.quantity + 1 } : line)
          : [...state.lines, snapshotProduct(product)],
      }));
    },
    increment(productId: string) {
      patchState(store, state => ({ lines: state.lines.map(line => line.productId === productId ? { ...line, quantity: line.quantity + 1 } : line) }));
    },
    decrement(productId: string) {
      patchState(store, state => ({ lines: state.lines.filter(line => line.productId !== productId || line.quantity > 1).map(line => line.productId === productId ? { ...line, quantity: line.quantity - 1 } : line) }));
    },
    setQuantity(productId: string, quantity: number) {
      patchState(store, state => ({ lines: state.lines.flatMap(line => line.productId !== productId ? [line] : quantity < 1 ? [] : [{ ...line, quantity }]) }));
    },
    remove(productId: string) {
      patchState(store, state => ({ lines: state.lines.filter(line => line.productId !== productId) }));
    },
    clear() {
      patchState(store, initialState);
    },
  })),
);
