import { computed, inject } from '@angular/core';
import { signalStore, withComputed } from '@ngrx/signals';
import { SalesStore } from '../../sales/state/sales.store';

export const ReportsStore = signalStore(
  {providedIn:'root'},
  withComputed(()=>{
    const sales=inject(SalesStore);
    return {summary:computed(()=>{
      const transactions=sales.sales();
      const revenue=transactions.reduce((sum,sale)=>sum+sale.totalCents,0);
      return {revenue,transactionCount:transactions.length,average:transactions.length?Math.round(revenue/transactions.length):0,cashCount:transactions.filter(sale=>sale.paymentMethod==='cash').length};
    })};
  }),
);
