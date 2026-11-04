export interface CartLine {
  productId: string;
  name: string;
  sku: string;
  unit: string;
  unitPriceCents: number;
  taxRate: number;
  quantity: number;
}
