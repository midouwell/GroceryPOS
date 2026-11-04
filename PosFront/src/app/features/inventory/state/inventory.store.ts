import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { ProductsStore } from '../../products/state/products.store';
import { INVENTORY_DATA_PORT } from '../services/inventory-data.service';

interface InventoryState { lastAdjustment: string | null; }
export const InventoryStore = signalStore(
  { providedIn: 'root' },
  withState<InventoryState>({ lastAdjustment: null }),
  withComputed(() => {
    const products = inject(ProductsStore);
    return {
      lowStockProducts: computed(() => products.lowStockProducts()),
      outOfStockProducts: computed(() => products.outOfStockProducts()),
    };
  }),
  withMethods(store => {
    const products = inject(ProductsStore);
    const inventoryData = inject(INVENTORY_DATA_PORT);
    return {
      adjustStock(productId: string, delta: number) {
        const current = products.products().find(product => product.id === productId)?.stockQuantity;
        if (current === undefined) return;
        products.setStock(productId, inventoryData.applyAdjustment(current,delta));
        patchState(store, { lastAdjustment: productId });
      },
    };
  }),
);
