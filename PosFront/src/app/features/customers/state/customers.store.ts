import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { Customer } from '../../../core/models/customer.model';

const demoCustomers: Customer[] = [
  {id:'c-1001',name:'Amelia Chen',email:'amelia.chen@example.test',phone:'(416) 555-0142',visitCount:28,lifetimeSpendCents:184290,lastVisit:'2026-10-26'},
  {id:'c-1002',name:'Noah Williams',email:'noah.w@example.test',phone:'(647) 555-0184',visitCount:16,lifetimeSpendCents:97650,lastVisit:'2026-10-24'},
  {id:'c-1003',name:'Sofia Martin',email:'sofia.m@example.test',phone:'(416) 555-0133',visitCount:42,lifetimeSpendCents:312810,lastVisit:'2026-10-23'},
  {id:'c-1004',name:'Liam Patel',email:'liam.patel@example.test',phone:'(905) 555-0117',visitCount:9,lifetimeSpendCents:54880,lastVisit:'2026-10-21'},
];
export const CustomersStore = signalStore(
  {providedIn:'root'},
  withState({customers:demoCustomers}),
  withMethods(store=>({addCustomer(customer:Customer){patchState(store,state=>({customers:[customer,...state.customers]}));}})),
);
