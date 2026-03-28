import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuditLog } from '../models/audit-log.model';
import { PurchaseBill } from '../models/purchase-bill.model';
import { PurchaseBillListItem } from '../models/purchase-bill-list-item.model';
import { SavePurchaseBillRequest } from '../models/save-purchase-bill-request.model';

@Injectable({ providedIn: 'root' })
export class PurchaseBillService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = environment.apiBaseUrl;

  getPurchaseBills(): Observable<PurchaseBillListItem[]> {
    return this.http.get<PurchaseBillListItem[]>(`${this.apiBaseUrl}/purchase-bill`);
  }

  getPurchaseBill(id: number): Observable<PurchaseBill> {
    return this.http.get<PurchaseBill>(`${this.apiBaseUrl}/purchase-bill/${id}`);
  }

  createPurchaseBill(payload: SavePurchaseBillRequest): Observable<PurchaseBill> {
    return this.http.post<PurchaseBill>(`${this.apiBaseUrl}/purchase-bill`, payload);
  }

  updatePurchaseBill(id: number, payload: SavePurchaseBillRequest): Observable<PurchaseBill> {
    return this.http.put<PurchaseBill>(`${this.apiBaseUrl}/purchase-bill/${id}`, payload);
  }

  getAuditLogs(): Observable<AuditLog[]> {
    return this.http.get<AuditLog[]>(`${this.apiBaseUrl}/audit-logs`);
  }
}
