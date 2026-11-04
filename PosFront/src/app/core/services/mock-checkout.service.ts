import { Injectable } from '@angular/core';
import { CartLine, Sale } from '../models/pos.models';

@Injectable({ providedIn: 'root' })
export class MockCheckoutService {
  recordCashSale(
    lines: CartLine[],
    subtotalCents: number,
    taxCents: number,
    tenderedCents: number,
  ): Sale {
    const totalCents = subtotalCents + taxCents;
    if (!lines.length) throw new Error('Add at least one item before checkout.');
    if (tenderedCents < totalCents) throw new Error('Amount tendered is less than the amount due.');
    const now = new Date();
    return {
      id: globalThis.crypto?.randomUUID?.() ?? `${now.getTime()}`,
      saleNumber: `S-${now.getTime().toString().slice(-6)}`,
      date: now.toISOString(),
      lines: lines.map((l) => ({ ...l })),
      subtotalCents,
      taxCents,
      totalCents,
      paymentMethod: 'cash',
      status: 'completed',
    };
  }
}
