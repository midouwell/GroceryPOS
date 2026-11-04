import { TestBed } from '@angular/core/testing';
import { CartStore } from './cart.store';
import { CheckoutStore } from './checkout.store';
import { SalesStore } from '../../sales/state/sales.store';
import { seedProducts } from '../../../core/models/pos.models';

describe('CheckoutStore', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('keeps the cart after insufficient tender and records then clears after success', async () => {
    vi.useFakeTimers();
    const cart=TestBed.inject(CartStore);
    const checkout=TestBed.inject(CheckoutStore);
    const sales=TestBed.inject(SalesStore);
    cart.addProduct(seedProducts[0]);
    const salesBefore=sales.sales().length;

    checkout.submitCash(100);
    await vi.advanceTimersByTimeAsync(251);
    expect(checkout.status()).toBe('failed');
    expect(cart.itemCount()).toBe(1);
    expect(sales.sales()).toHaveLength(salesBefore);

    checkout.reset();
    checkout.submitCash(500);
    checkout.submitCash(500);
    await vi.advanceTimersByTimeAsync(251);
    expect(checkout.status()).toBe('succeeded');
    expect(checkout.receipt()?.totalCents).toBe(449);
    expect(cart.isEmpty()).toBe(true);
    expect(sales.sales()).toHaveLength(salesBefore+1);
    vi.useRealTimers();
  });
});
