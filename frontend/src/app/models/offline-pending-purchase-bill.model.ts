import { SavePurchaseBillRequest } from './save-purchase-bill-request.model';

export interface OfflinePendingPurchaseBill {
  queueId: string;
  operation: 'create' | 'update';
  purchaseBillId?: number;
  payload: SavePurchaseBillRequest;
  savedAt: string;
  syncStatus: 'Pending' | 'Synced';
}
