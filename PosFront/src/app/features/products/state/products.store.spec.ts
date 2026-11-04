import { TestBed } from '@angular/core/testing';
import { ProductsStore } from './products.store';
import { InventoryStore } from '../../inventory/state/inventory.store';
import { seedProducts } from '../../../core/models/pos.models';

describe('ProductsStore',()=>{
  beforeEach(()=>TestBed.configureTestingModule({}));
  it('exposes low and out-of-stock products as computed state',()=>{
    const products=TestBed.inject(ProductsStore);
    expect(products.lowStockProducts().map(product=>product.id)).toContain('p2');
    expect(products.outOfStockProducts().map(product=>product.id)).toContain('p8');
  });
  it('updates product and inventory state without mutating the seeded object',()=>{
    const products=TestBed.inject(ProductsStore);
    const inventory=TestBed.inject(InventoryStore);
    const original=products.products().find(product=>product.id==='p1')!;
    inventory.adjustStock('p1',-3);
    products.toggleProduct('p1');
    expect(products.products().find(product=>product.id==='p1')).toMatchObject({stockQuantity:15,isActive:false});
    expect(original).toMatchObject({stockQuantity:18,isActive:true});
    products.saveProduct({...seedProducts[0],name:'Updated milk'});
    expect(products.products().find(product=>product.id==='p1')?.name).toBe('Updated milk');
  });
});
