import { computed, inject } from '@angular/core';
import { signalStore, withComputed } from '@ngrx/signals';
import { ProductsStore } from '../../products/state/products.store';
import { SalesStore } from '../../sales/state/sales.store';

export const DashboardStore = signalStore(
  { providedIn: 'root' },
  withComputed(() => {
    const products = inject(ProductsStore);
    const sales = inject(SalesStore);
    return {
      metrics: computed(() => {
        const today = new Date().toDateString();
        const todaysSales = sales.sales().filter(sale => new Date(sale.date).toDateString() === today);
        const revenue = todaysSales.reduce((sum, sale) => sum + sale.totalCents, 0);
        return {
          revenue,
          salesCount: todaysSales.length,
          average: todaysSales.length ? Math.round(revenue / todaysSales.length) : 0,
          productCount: products.products().length,
          lowCount: products.lowStockProducts().length,
        };
      }),
    };
  }),
);
