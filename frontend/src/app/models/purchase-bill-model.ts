import { PurchaseBillLine } from './purchase-bill-line.model';

export interface PurchaseBill {
  id: number;
  billNumber: string;
  purchaseDate: string;
  supplierName: string;
  referenceNo: string;
  notes: string;
  syncStatus: string;
  offlineClientId?: string | null;
  totalItems: number;
  totalQuantity: number;
  totalAmount: number;
  totalCost: number;
  createdAt: string;
  updatedAt: string;
  items: PurchaseBillLine[];
}
