import { AsyncPipe, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { combineLatest, map, take } from 'rxjs';
import { Product, Sale } from '../../core/models/pos.models';
import { CartStore } from './state/cart.store';
import { CheckoutStore } from './state/checkout.store';
import { ProductsStore } from '../products/state/products.store';
import { BarcodeScanDirective } from '../../shared/directives/barcode-scan.directive';
import { PAYMENT_GATEWAY } from './services/payment.gateway';

@Component({selector:'app-checkout-page',imports:[AsyncPipe,CurrencyPipe,DatePipe,FormsModule,RouterLink,BarcodeScanDirective],templateUrl:'./checkout.page.html'})
export class CheckoutPage {
 private cart=inject(CartStore);
 private checkout=inject(CheckoutStore);
 private products=inject(ProductsStore);
 private paymentGateway=inject(PAYMENT_GATEWAY);
 products$=toObservable(this.products.products);
 lines$=toObservable(this.cart.lines);
 count$=toObservable(this.cart.itemCount);
 empty$=toObservable(this.cart.isEmpty);
 subtotal$=toObservable(this.cart.subtotalCents);
 tax$=toObservable(this.cart.taxCents);
 total$=toObservable(this.cart.totalCents);
 checkout$=combineLatest([toObservable(this.checkout.status),toObservable(this.checkout.error),toObservable(this.checkout.receipt)])
  .pipe(map(([status,error,lastSale])=>({status,error,lastSale})));
 categories=['All categories','Pantry','Beverages','Dairy','Bakery','Produce'];
 category=signal('All categories'); search=signal(''); barcode=signal(''); tendered=signal('');
 selected=signal<string|null>(null); keypadMode=signal<'barcode'|'quantity'>('barcode');
 showCash=signal(false); showCard=signal(false); confirmClear=signal(false); notice=signal(''); error=signal('');
 today=new Date();
 cardMessage=signal(this.paymentGateway.cardStatus().message);
 openCard(){this.cardMessage.set(this.paymentGateway.cardStatus().message);this.showCard.set(true);}
 bySearch(product:Product){const query=(this.search()||this.barcode()).trim().toLowerCase();return product.isActive&&(!query||[product.name,product.sku,product.barcode,product.category].some(value=>value.toLowerCase().includes(query)))&&(this.category()==='All categories'||product.category===this.category());}
 productEmoji(product:Product){const name=product.name.toLowerCase();if(name.includes('milk'))return '\u{1F95B}';if(name.includes('bread')||name.includes('sourdough'))return '\u{1F35E}';if(name.includes('avocado'))return '\u{1F951}';if(name.includes('egg'))return '\u{1F95A}';if(name.includes('coffee'))return '\u{2615}';if(name.includes('spinach'))return '\u{1F96C}';if(name.includes('water'))return '\u{1F964}';if(name.includes('banana'))return '\u{1F34C}';if(name.includes('cheese'))return '\u{1F9C0}';return product.category==='Produce'?'\u{1F966}':product.category==='Bakery'?'\u{1F950}':product.category==='Dairy'?'\u{1F9C0}':'\u{1F96B}';}
 add(product:Product){this.cart.addProduct(product);this.selected.set(product.id);this.notice.set(product.name+' added to order');}
 scan(scannedCode?:string){const code=(scannedCode??this.barcode()).trim();if(!code)return;this.products$.pipe(take(1)).subscribe(products=>{const exact=products.find(product=>product.barcode===code||product.sku.toLowerCase()===code.toLowerCase()||product.name.toLowerCase()===code.toLowerCase());const matches=products.filter(product=>product.isActive&&product.name.toLowerCase().includes(code.toLowerCase()));const product=exact??(matches.length===1?matches[0]:undefined);if(!product||!product.isActive){if(matches.length>1){this.search.set(code);this.error.set('Choose a matching product from the results.');}else this.error.set(exact?'This product is inactive.':'No product matches that search or barcode.');}else{this.add(product);this.error.set('');this.search.set('');}this.barcode.set('');document.getElementById('product-search')?.focus();});}
 quantity(productId:string,delta:number){if(delta<0)this.cart.decrement(productId);else this.cart.increment(productId);}
 remove(productId:string){this.cart.remove(productId);if(this.selected()===productId)this.selected.set(null);}
 clear(){this.confirmClear.set(true);}
 confirmClearOrder(){this.cart.clear();this.confirmClear.set(false);this.notice.set('Order cleared');}
 key(key:string){if(key==='Qty'){this.keypadMode.set('quantity');this.barcode.set('');return;}if(key==='Backspace'){this.barcode.update(value=>value.slice(0,-1));return;}if(key==='Enter'){this.enterKeypad();return;}if(key==='-')return;this.barcode.update(value=>value+key);}
 enterKeypad(){if(this.keypadMode()==='quantity'&&this.selected()){const quantity=Math.floor(Number(this.barcode()));if(quantity>0)this.cart.setQuantity(this.selected()!,quantity);this.barcode.set('');this.keypadMode.set('barcode');return;}this.scan();}
 openCash(){this.error.set('');this.tendered.set('');this.checkout.reset();this.showCash.set(true);}
 setTendered(value:string){this.tendered.set(value);this.checkout.setCashTendered(Math.round(Number(value)*100)||0);}
 confirmCash(){const tenderedCents=Math.round(Number(this.tendered())*100);if(!Number.isFinite(tenderedCents)||tenderedCents<this.cart.totalCents()){this.error.set('Enter enough cash to cover the total.');return;}this.error.set('');this.checkout.submitCash(tenderedCents);}
 closeCash(){this.showCash.set(false);this.error.set('');this.checkout.reset();}
 change(total:number){return Math.max(0,this.checkout.cashTenderedCents()-total)/100;}
 done(){this.showCash.set(false);this.checkout.reset();}
 saleItems(sale:Sale){return sale.lines.reduce((sum,line)=>sum+line.quantity,0);}
}
