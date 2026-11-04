import { TestBed } from '@angular/core/testing';
import { CartStore } from './cart.store';
import { seedProducts } from '../../../core/models/pos.models';

describe('CartStore', () => {
  let cart: InstanceType<typeof CartStore>;
  beforeEach(() => {
    TestBed.configureTestingModule({});
    cart = TestBed.inject(CartStore);
  });

  it('adds product snapshots and merges repeated scans', () => {
    cart.addProduct(seedProducts[0]);
    cart.addProduct(seedProducts[0]);
    expect(cart.lines()).toHaveLength(1);
    expect(cart.lines()[0].quantity).toBe(2);
    expect(cart.lines()[0].unitPriceCents).toBe(449);
  });

  it('calculates subtotal, per-line tax, and total in cents', () => {
    cart.addProduct(seedProducts.find(product => product.id === 'p5')!);
    cart.addProduct(seedProducts.find(product => product.id === 'p5')!);
    expect(cart.subtotalCents()).toBe(2500);
    expect(cart.taxCents()).toBe(325);
    expect(cart.totalCents()).toBe(2825);
  });

  it('removes an item when its quantity is decremented to zero', () => {
    cart.addProduct(seedProducts[0]);
    cart.decrement(seedProducts[0].id);
    expect(cart.lines()).toEqual([]);
    expect(cart.isEmpty()).toBe(true);
    expect(cart.checkoutAvailable()).toBe(false);
  });

  it('ignores inactive products and clamps stock-independent cart quantities', () => {
    cart.addProduct({...seedProducts[0],isActive:false});
    expect(cart.itemCount()).toBe(0);
    cart.addProduct(seedProducts[0]);
    cart.setQuantity(seedProducts[0].id,0);
    expect(cart.lines()).toEqual([]);
  });
});
