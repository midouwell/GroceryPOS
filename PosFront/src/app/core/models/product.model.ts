export interface Product {
  id: string;
  sku: string;
  barcode: string;
  name: string;
  description: string;
  category: string;
  unit: string;
  price: number;
  taxRate: number;
  stockQuantity: number;
  reorderLevel: number;
  isActive: boolean;
}
