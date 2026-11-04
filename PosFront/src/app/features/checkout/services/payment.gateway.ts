import { Injectable, InjectionToken } from '@angular/core';

export interface CardGatewayStatus {
  available: boolean;
  message: string;
}
export interface PaymentGateway {
  cardStatus(): CardGatewayStatus;
}

/** No terminal is configured; this adapter never authorizes a card payment. */
@Injectable()
export class MockPaymentGateway implements PaymentGateway {
  cardStatus(): CardGatewayStatus {
    return {available:false,message:'Terminal integration will be added in a future version.'};
  }
}
export const PAYMENT_GATEWAY = new InjectionToken<PaymentGateway>('PAYMENT_GATEWAY',{
  providedIn:'root',
  factory:()=>new MockPaymentGateway(),
});
