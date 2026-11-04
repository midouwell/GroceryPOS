import { Injectable, InjectionToken } from '@angular/core';

export interface InventoryDataPort {
  applyAdjustment(currentQuantity: number, delta: number): number;
}

export const INVENTORY_DATA_PORT = new InjectionToken<InventoryDataPort>('INVENTORY_DATA_PORT',{providedIn:'root',factory:()=>new MockInventoryDataService()});

/** Mock stock operation; persistence and authoritative checks belong to the API. */
@Injectable()
export class MockInventoryDataService implements InventoryDataPort {
  applyAdjustment(currentQuantity: number, delta: number): number {
    return Math.max(0, currentQuantity + Math.trunc(delta));
  }
}
