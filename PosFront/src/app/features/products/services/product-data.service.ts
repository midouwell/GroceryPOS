import { Injectable, InjectionToken } from '@angular/core';
import { Product } from '../../../core/models/product.model';

export interface ProductDataPort {
  save(product: Product): Product;
  setActive(product: Product, isActive: boolean): Product;
}

export const PRODUCT_DATA_PORT = new InjectionToken<ProductDataPort>('PRODUCT_DATA_PORT',{providedIn:'root',factory:()=>new MockProductDataService()});

/** Demo mapper boundary. The SignalStore owns the in-memory collection. */
@Injectable()
export class MockProductDataService implements ProductDataPort {
  save(product: Product): Product { return { ...product }; }
  setActive(product: Product, isActive: boolean): Product { return { ...product, isActive }; }
}
