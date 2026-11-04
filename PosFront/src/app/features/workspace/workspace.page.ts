import { AsyncPipe, CurrencyPipe, DatePipe, UpperCasePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink, RouterLinkActive } from '@angular/router';
import { toObservable } from '@angular/core/rxjs-interop';
import { combineLatest, map, take } from 'rxjs';
import { Product, Sale } from '../../core/models/pos.models';
import { CartStore } from '../checkout/state/cart.store';
import { CheckoutStore } from '../checkout/state/checkout.store';
import { DashboardStore } from '../dashboard/state/dashboard.store';
import { InventoryStore } from '../inventory/state/inventory.store';
import { ProductsStore } from '../products/state/products.store';
import { SalesStore } from '../sales/state/sales.store';
import { CustomersStore } from '../customers/state/customers.store';
import { ReportsStore } from '../reports/state/reports.store';

@Component({selector:'app-workspace-page',imports:[AsyncPipe,CurrencyPipe,DatePipe,UpperCasePipe,FormsModule,RouterLink,RouterLinkActive],templateUrl:'./workspace.page.html'})
export class WorkspacePage {
 private route=inject(ActivatedRoute);
 private productsStore=inject(ProductsStore);
 private cartStore=inject(CartStore);
 private checkoutStore=inject(CheckoutStore);
 private dashboardStore=inject(DashboardStore);
 private inventoryStore=inject(InventoryStore);
 private salesStore=inject(SalesStore);
 private customersStore=inject(CustomersStore);
 private reportsStore=inject(ReportsStore);

 today=new Date();
 page$=this.route.data.pipe(map(data=>String(data['page'])));
 products$=toObservable(this.productsStore.products);
 lines$=toObservable(this.cartStore.lines);
 sales$=toObservable(this.salesStore.sales);
 customers$=toObservable(this.customersStore.customers);
 reports$=toObservable(this.reportsStore.summary);
 low$=toObservable(this.inventoryStore.lowStockProducts);
 dashboard$=toObservable(this.dashboardStore.metrics);
 subtotal$=toObservable(this.cartStore.subtotalCents);
 tax$=toObservable(this.cartStore.taxCents);
 total$=toObservable(this.cartStore.totalCents);
 count$=toObservable(this.cartStore.itemCount);
 empty$=toObservable(this.cartStore.isEmpty);
 checkout$=combineLatest([
  toObservable(this.checkoutStore.status),
  toObservable(this.checkoutStore.error),
  toObservable(this.checkoutStore.receipt),
 ]).pipe(map(([status,error,lastSale])=>({status,error,lastSale})));

 query=signal(''); category=signal('All categories'); barcode=signal(''); errorMessage=signal(''); noticeMessage=signal('');
 showCash=signal(false); showCard=signal(false); confirmClear=signal(false); saleDate=signal(''); saleMethod=signal('All payment methods');
 tendered=signal(''); stockTarget=signal<Product|null>(null); stockAmount=signal(''); showProductForm=signal(false);
 editing=signal<Product|null>(null); details=signal<Sale|null>(null);
 draft=signal<Product>({id:'',sku:'',barcode:'',name:'',description:'',category:'Produce',unit:'each',price:0,taxRate:0,stockQuantity:0,reorderLevel:0,isActive:true});
 categories=['All categories','Dairy','Bakery','Produce','Pantry','Beverages'];
 headings:Record<string,string>={dashboard:'Dashboard',checkout:'Point of sale',products:'Product catalog',inventory:'Inventory',sales:'Sales history',customers:'Customers',reports:'Reports'};
 heading(page:string|null){return this.headings[page??'dashboard']??'Dashboard';}
 icon(page:string){return ({dashboard:'D',checkout:'C',products:'P',inventory:'I',sales:'S',customers:'C',reports:'R'} as Record<string,string>)[page]??'•';}
 bySearch(product:Product){const query=this.query().trim().toLowerCase();return (!query||[product.name,product.sku,product.barcode,product.category].some(value=>value.toLowerCase().includes(query)))&&(this.category()==='All categories'||product.category===this.category());}
 scan(){const code=this.barcode().trim();if(!code)return;this.products$.pipe(take(1)).subscribe(products=>{const product=products.find(item=>item.barcode===code||item.sku.toLowerCase()===code.toLowerCase());if(!product||!product.isActive)this.errorMessage.set(product?'This product is inactive.':'No active product matches that barcode.');else{this.cartStore.addProduct(product);this.errorMessage.set('');this.noticeMessage.set(product.name+' added to the cart');}this.barcode.set('');document.getElementById('barcode-input')?.focus();});}
 add(product:Product){this.cartStore.addProduct(product);this.noticeMessage.set(product.name+' added to the cart');}
 clear(){this.confirmClear.set(true);}
 confirmClearCart(){this.cartStore.clear();this.confirmClear.set(false);this.noticeMessage.set('Cart cleared');}
 changeQuantity(productId:string,delta:number){if(delta>0)this.cartStore.increment(productId);else this.cartStore.decrement(productId);}
 removeProductLine(productId:string){this.cartStore.remove(productId);}
 toggleActive(productId:string){this.productsStore.toggleProduct(productId);}
 pay(){this.errorMessage.set('');this.tendered.set('');this.checkoutStore.reset();this.showCash.set(true);}
 confirmCash(total:number){const cents=Math.round(Number(this.tendered())*100);if(!Number.isFinite(cents)||cents<total){this.errorMessage.set('Enter an amount that covers the total.');return;}this.checkoutStore.submitCash(cents);this.errorMessage.set('');}
 confirmCashCurrent(){this.confirmCash(this.cartStore.totalCents());}
 change(total:number){const cents=Math.round(Number(this.tendered())*100)-total;return Math.max(cents,0)/100;}
 closeCash(){this.showCash.set(false);this.errorMessage.set('');this.checkoutStore.reset();}
 dismissReceipt(){this.checkoutStore.reset();this.showCash.set(false);}
 saveProductForm(form:Product){if(!form.name?.trim()||!form.sku?.trim()||!Number.isFinite(Number(form.price))||Number(form.price)<=0){this.errorMessage.set('Enter a name, SKU, and a price above zero.');return;}const product={...form,id:form.id||('p'+Date.now()),barcode:form.barcode||form.sku,description:form.description||'',category:form.category||'Produce',unit:form.unit||'each',price:Number(form.price),taxRate:Number(form.taxRate)||0,stockQuantity:Number(form.stockQuantity)||0,reorderLevel:Number(form.reorderLevel)||0,isActive:form.isActive??true};this.productsStore.saveProduct(product);this.showProductForm.set(false);this.editing.set(null);this.noticeMessage.set('Product updated in this demo session.');this.errorMessage.set('');}
 edit(product?:Product){this.editing.set(product??null);this.draft.set(product?{...product}:{id:'',sku:'',barcode:'',name:'',description:'',category:'Produce',unit:'each',price:0,taxRate:0,stockQuantity:0,reorderLevel:0,isActive:true});this.showProductForm.set(true);this.errorMessage.set('');}
 updateDraft(field:keyof Product,value:string|number){this.draft.update(product=>({...product,[field]:value}));}
 openStock(product:Product){this.stockTarget.set(product);this.stockAmount.set('');}
 adjust(product:Product,delta:number){this.inventoryStore.adjustStock(product.id,delta);this.stockTarget.set(null);this.noticeMessage.set('Stock adjustment applied to demo data.');}
 stockStatus(product:Product){return product.stockQuantity===0?'Out of stock':product.stockQuantity<=product.reorderLevel?'Low stock':'In stock';}
 stockClass(product:Product){return product.stockQuantity===0?'out':product.stockQuantity<=product.reorderLevel?'low':'good';}
 saleItems(sale:Sale){return sale.lines.reduce((count,line)=>count+line.quantity,0);}
 matchesSale(sale:Sale){return (!this.query()||sale.saleNumber.toLowerCase().includes(this.query().toLowerCase()))&&(!this.saleDate()||sale.date.slice(0,10)===this.saleDate())&&(this.saleMethod()==='All payment methods'||sale.paymentMethod===this.saleMethod().toLowerCase());}
 matchesCustomer(customer:{name:string;email:string;phone:string}){const query=this.query().toLowerCase();return !query||[customer.name,customer.email,customer.phone].some(value=>value.toLowerCase().includes(query));}
}
