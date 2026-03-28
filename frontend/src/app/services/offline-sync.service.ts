import { Injectable } from '@angular/core';
import { Observable, catchError, concatMap, from, last, map, of } from 'rxjs';
import { OfflinePendingPurchaseBill } from '../models/offline-pending-purchase-bill.model';
import { PurchaseBillService } from './purchase-bill.service';

const STORAGE_KEY = 'enhanzer-offline-purchase-bills';

@Injectable({ providedIn: 'root' })
export class OfflineSyncService {
  getPendingQueue(): OfflinePendingPurchaseBill[] {
    const rawValue = localStorage.getItem(STORAGE_KEY);
    if (!rawValue) {
      return [];
    }

    try {
      return JSON.parse(rawValue) as OfflinePendingPurchaseBill[];
    } catch {
      return [];
    }
  }

  enqueue(entry: OfflinePendingPurchaseBill): void {
    const queue = this.getPendingQueue();
    const duplicateIndex = queue.findIndex((item) =>
      (entry.operation === 'create' && item.payload.offlineClientId && item.payload.offlineClientId === entry.payload.offlineClientId) ||
      (entry.operation === 'update' && item.purchaseBillId === entry.purchaseBillId));

    if (duplicateIndex >= 0) {
      queue[duplicateIndex] = entry;
    } else {
      queue.push(entry);
    }

    localStorage.setItem(STORAGE_KEY, JSON.stringify(queue));
  }

  remove(queueId: string): void {
    const queue = this.getPendingQueue().filter((entry) => entry.queueId !== queueId);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(queue));
  }

  syncPending(purchaseBillService: PurchaseBillService): Observable<number> {
    const queue = this.getPendingQueue();
    if (!queue.length) {
      return of(0);
    }

    let syncedCount = 0;

    return from(queue).pipe(
      concatMap((entry) => {
        const request = entry.operation === 'update' && entry.purchaseBillId
          ? purchaseBillService.updatePurchaseBill(entry.purchaseBillId, entry.payload)
          : purchaseBillService.createPurchaseBill(entry.payload);

        return request.pipe(
          map(() => {
            syncedCount += 1;
            this.remove(entry.queueId);
            return syncedCount;
          }),
          catchError(() => of(syncedCount))
        );
      }),
      last(undefined, syncedCount)
    );
  }
}
