import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { Product, seedProducts } from '../../../core/models/pos.models';
import { PRODUCT_DATA_PORT } from '../services/product-data.service';

interface ProductsState { products: Product[]; selectedProductId: string | null; }
const initialState: ProductsState = { products: seedProducts, selectedProductId: null };

export const ProductsStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed(({ products }) => ({
    activeProducts: computed(() => products().filter(product => product.isActive)),
    lowStockProducts: computed(() => products().filter(product => product.stockQuantity <= product.reorderLevel)),
    outOfStockProducts: computed(() => products().filter(product => product.stockQuantity === 0)),
  })),
  withMethods(store => {
    const productData = inject(PRODUCT_DATA_PORT);
    return {
    saveProduct(product: Product) {
      const saved = productData.save(product);
      patchState(store, state => ({
        products: state.products.some(item => item.id === saved.id)
          ? state.products.map(item => item.id === saved.id ? saved : item)
          : [saved, ...state.products],
      }));
    },
    toggleProduct(productId: string) {
      patchState(store, state => ({
        products: state.products.map(product => product.id === productId ? productData.setActive(product,!product.isActive) : product),
      }));
    },
    setStock(productId: string, stockQuantity: number) {
      patchState(store, state => ({
        products: state.products.map(product => product.id === productId
          ? { ...product, stockQuantity }
          : product),
      }));
    },
    selectProduct(productId: string | null) {
      patchState(store, { selectedProductId: productId });
    },
    };
  }),
);
