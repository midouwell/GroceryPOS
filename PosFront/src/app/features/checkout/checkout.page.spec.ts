import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { CheckoutPage } from './checkout.page';
import { CartStore } from './state/cart.store';

describe('CheckoutPage barcode lookup',()=>{
  beforeEach(()=>TestBed.configureTestingModule({providers:[provideRouter([])]}));
  it('adds the active product matching a scanned barcode',async()=>{
    const fixture=TestBed.createComponent(CheckoutPage);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.componentInstance.scan('628100000001');
    await fixture.whenStable();
    expect(TestBed.inject(CartStore).lines()[0].productId).toBe('p1');
  });
  it('reports an unknown barcode without changing the cart',async()=>{
    const fixture=TestBed.createComponent(CheckoutPage);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.componentInstance.scan('unknown-code');
    await fixture.whenStable();
    expect(fixture.componentInstance.error()).toContain('No product matches');
    expect(TestBed.inject(CartStore).isEmpty()).toBe(true);
  });
});
