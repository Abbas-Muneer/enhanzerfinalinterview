import { PurchaseBillLine } from './purchase-bill-line.model';

export interface SavePurchaseBillRequest {
  id?: number | null;
  billNumber?: string | null;
  offlineClientId?: string | null;
  purchaseDate: string;
  supplierName: string;
  referenceNo: string;
  notes: string;
  syncStatus: string;
  items: PurchaseBillLine[];
}
