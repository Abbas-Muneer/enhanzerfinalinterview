import { CommonModule, DatePipe } from '@angular/common';
import { Component, DestroyRef, HostListener, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import { MasterDataService } from '../../../../core/services/master-data.service';
import { AuditLog } from '../../../../models/audit-log.model';
import { Item } from '../../../../models/item.model';
import { Location } from '../../../../models/location.model';
import { OfflinePendingPurchaseBill } from '../../../../models/offline-pending-purchase-bill.model';
import { PurchaseBillLine } from '../../../../models/purchase-bill-line.model';
import { PurchaseBillListItem } from '../../../../models/purchase-bill-list-item.model';
import { PurchaseBill } from '../../../../models/purchase-bill.model';
import { SavePurchaseBillRequest } from '../../../../models/save-purchase-bill-request.model';
import { summarizePurchaseBill, calculateLineTotals } from '../../../../shared/utils/purchase-bill-calculator';
import { OfflineSyncService } from '../../../../services/offline-sync.service';
import { PurchaseBillService } from '../../../../services/purchase-bill.service';

@Component({
  selector: 'app-purchase-bill',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DatePipe],
  templateUrl: './purchase-bill.component.html',
  styleUrl: './purchase-bill.component.css'
})
export class PurchaseBillComponent {
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly masterDataService = inject(MasterDataService);
  private readonly purchaseBillService = inject(PurchaseBillService);
  private readonly offlineSyncService = inject(OfflineSyncService);

  protected readonly itemsMaster = signal<Item[]>([]);
  protected readonly locations = signal<Location[]>([]);
  protected readonly savedBills = signal<PurchaseBillListItem[]>([]);
  protected readonly auditLogs = signal<AuditLog[]>([]);
  protected readonly itemRows = signal<PurchaseBillLine[]>([]);
  protected readonly filteredItems = signal<Item[]>([]);
  protected readonly isSaving = signal(false);
  protected readonly isLoading = signal(true);
  protected readonly statusMessage = signal('');
  protected readonly errorMessage = signal('');
  protected readonly selectedBillId = signal<number | null>(null);
  protected readonly editingLineIndex = signal<number | null>(null);
  protected readonly isOnline = signal(navigator.onLine);
  protected readonly pendingQueue = signal<OfflinePendingPurchaseBill[]>([]);

  protected readonly headerForm = this.fb.group({
    purchaseDate: [this.todayString(), Validators.required],
    supplierName: ['', Validators.required],
    referenceNo: [''],
    notes: ['']
  });

  protected readonly lineForm = this.fb.group({
    itemName: ['', Validators.required],
    batchCode: ['', Validators.required],
    cost: [0, [Validators.required, Validators.min(0)]],
    price: [0, [Validators.required, Validators.min(0)]],
    quantity: [1, [Validators.required, Validators.min(0.01)]],
    discountPercentage: [0, [Validators.required, Validators.min(0), Validators.max(100)]],
    totalCost: [{ value: 0, disabled: true }],
    totalSelling: [{ value: 0, disabled: true }]
  });

  protected readonly summary = computed(() => summarizePurchaseBill(this.itemRows()));

  constructor() {
    this.loadScreenData();
    this.setupLineCalculations();
    this.setupItemAutocomplete();
    this.pendingQueue.set(this.offlineSyncService.getPendingQueue());
  }

  @HostListener('window:online')
  protected handleOnline(): void {
    this.isOnline.set(true);
    this.syncPendingBills();
  }

  @HostListener('window:offline')
  protected handleOffline(): void {
    this.isOnline.set(false);
    this.statusMessage.set('You are offline. New changes can be queued locally.');
  }

  protected addOrUpdateLine(): void {
    if (this.lineForm.invalid) {
      this.lineForm.markAllAsTouched();
      return;
    }

    const rawValue = this.lineForm.getRawValue();
    const line: PurchaseBillLine = {
      itemName: (rawValue.itemName ?? '').trim(),
      batchCode: rawValue.batchCode ?? '',
      cost: Number(rawValue.cost),
      price: Number(rawValue.price),
      quantity: Number(rawValue.quantity),
      discountPercentage: Number(rawValue.discountPercentage),
      totalCost: Number(rawValue.totalCost),
      totalSelling: Number(rawValue.totalSelling)
    };

    const editIndex = this.editingLineIndex();
    if (editIndex === null) {
      this.itemRows.update((rows) => [...rows, line]);
      this.statusMessage.set('Line item added.');
    } else {
      this.itemRows.update((rows) => rows.map((row, index) => index === editIndex ? line : row));
      this.editingLineIndex.set(null);
      this.statusMessage.set('Line item updated.');
    }

    this.resetLineForm();
  }

  protected editLine(index: number): void {
    const line = this.itemRows()[index];
    this.editingLineIndex.set(index);
    this.lineForm.patchValue({
      itemName: line.itemName,
      batchCode: line.batchCode,
      cost: line.cost,
      price: line.price,
      quantity: line.quantity,
      discountPercentage: line.discountPercentage,
      totalCost: line.totalCost,
      totalSelling: line.totalSelling
    });
  }

  protected deleteLine(index: number): void {
    this.itemRows.update((rows) => rows.filter((_, rowIndex) => rowIndex !== index));
    if (this.editingLineIndex() === index) {
      this.resetLineForm();
      this.editingLineIndex.set(null);
    }
  }

  protected savePurchaseBill(): void {
    if (!this.validateAll()) {
      return;
    }

    const payload = this.buildPayload('Synced');
    this.isSaving.set(true);
    this.errorMessage.set('');

    const request$ = this.selectedBillId()
      ? this.purchaseBillService.updatePurchaseBill(this.selectedBillId()!, payload)
      : this.purchaseBillService.createPurchaseBill(payload);

    request$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.isSaving.set(false);
          this.statusMessage.set(`Purchase bill ${response.billNumber} saved successfully.`);
          this.selectedBillId.set(response.id);
          this.reloadLists();
        },
        error: () => {
          this.isSaving.set(false);
          this.errorMessage.set('Unable to save the purchase bill.');
        }
      });
  }

  protected saveOffline(): void {
    if (!this.validateAll()) {
      return;
    }

    const payload = this.buildPayload('Pending');
    if (!payload.offlineClientId) {
      payload.offlineClientId = this.createOfflineId();
    }

    this.offlineSyncService.enqueue({
      queueId: this.createOfflineId(),
      operation: this.selectedBillId() ? 'update' : 'create',
      purchaseBillId: this.selectedBillId() ?? undefined,
      payload,
      savedAt: new Date().toISOString(),
      syncStatus: 'Pending'
    });

    this.pendingQueue.set(this.offlineSyncService.getPendingQueue());
    this.statusMessage.set('Purchase bill saved offline and queued for sync.');
  }

  protected loadPurchaseBill(id: number): void {
    this.purchaseBillService.getPurchaseBill(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (purchaseBill) => this.populatePurchaseBill(purchaseBill),
        error: () => this.errorMessage.set('Unable to load the selected purchase bill.')
      });
  }

  protected startNew(): void {
    this.selectedBillId.set(null);
    this.editingLineIndex.set(null);
    this.headerForm.reset({
      purchaseDate: this.todayString(),
      supplierName: '',
      referenceNo: '',
      notes: ''
    });
    this.itemRows.set([]);
    this.resetLineForm();
    this.statusMessage.set('Ready for a new purchase bill.');
  }

  protected syncPendingBills(): void {
    this.pendingQueue.set(this.offlineSyncService.getPendingQueue());
    if (!this.pendingQueue().length) {
      this.statusMessage.set('No pending offline bills to sync.');
      return;
    }

    this.offlineSyncService.syncPending(this.purchaseBillService)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (count) => {
          this.pendingQueue.set(this.offlineSyncService.getPendingQueue());
          this.reloadLists();
          this.statusMessage.set(`${count} offline bill(s) synced.`);
        },
        error: () => {
          this.pendingQueue.set(this.offlineSyncService.getPendingQueue());
          this.errorMessage.set('Offline sync failed. Pending queue remains intact.');
        }
      });
  }

  protected exportPdf(): void {
    if (!this.itemRows().length) {
      this.errorMessage.set('Add at least one line item before exporting.');
      return;
    }

    const document = new jsPDF();
    const summary = this.summary();
    const headerValues = this.headerForm.getRawValue();

    document.setFontSize(18);
    document.text('Purchase Bill', 14, 18);
    document.setFontSize(10);
    document.text(`Supplier: ${headerValues.supplierName ?? ''}`, 14, 28);
    document.text(`Purchase Date: ${headerValues.purchaseDate ?? ''}`, 14, 34);
    document.text(`Reference: ${headerValues.referenceNo ?? '-'}`, 14, 40);

    autoTable(document, {
      startY: 48,
      head: [['Item', 'Batch', 'Cost', 'Price', 'Qty', 'Discount %', 'Total Cost', 'Total Selling']],
      body: this.itemRows().map((row) => [
        row.itemName,
        row.batchCode,
        row.cost.toFixed(2),
        row.price.toFixed(2),
        row.quantity.toFixed(2),
        row.discountPercentage.toFixed(2),
        row.totalCost.toFixed(2),
        row.totalSelling.toFixed(2)
      ])
    });

    const finalY = (document as jsPDF & { lastAutoTable?: { finalY?: number } }).lastAutoTable?.finalY ?? 60;
    document.text(`Total Items: ${summary.totalItems}`, 14, finalY + 12);
    document.text(`Total Quantity: ${summary.totalQuantity.toFixed(2)}`, 14, finalY + 18);
    document.text(`Total Amount: ${summary.totalAmount.toFixed(2)}`, 14, finalY + 24);

    document.save(`${this.selectedBillId() ? 'purchase-bill-edit' : 'purchase-bill'}.pdf`);
  }

  protected selectItemSuggestion(item: Item): void {
    this.lineForm.patchValue({ itemName: item.name });
    this.filteredItems.set([]);
  }

  protected hasHeaderError(controlName: string, errorKey: string): boolean {
    const control = this.headerForm.get(controlName);
    return !!control && control.touched && control.hasError(errorKey);
  }

  protected hasLineError(controlName: string, errorKey: string): boolean {
    const control = this.lineForm.get(controlName);
    return !!control && control.touched && control.hasError(errorKey);
  }

  protected formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value);
  }

  private loadScreenData(): void {
    forkJoin({
      items: this.masterDataService.getItems(),
      locations: this.masterDataService.getLocations(),
      purchaseBills: this.purchaseBillService.getPurchaseBills().pipe(catchError(() => of([]))),
      auditLogs: this.purchaseBillService.getAuditLogs().pipe(catchError(() => of([])))
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ items, locations, purchaseBills, auditLogs }) => {
          this.itemsMaster.set(items);
          this.locations.set(locations);
          this.savedBills.set(purchaseBills);
          this.auditLogs.set(auditLogs);
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
          this.errorMessage.set('Unable to load purchase module data.');
        }
      });
  }

  private reloadLists(): void {
    forkJoin({
      purchaseBills: this.purchaseBillService.getPurchaseBills().pipe(catchError(() => of([]))),
      auditLogs: this.purchaseBillService.getAuditLogs().pipe(catchError(() => of([])))
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(({ purchaseBills, auditLogs }) => {
        this.savedBills.set(purchaseBills);
        this.auditLogs.set(auditLogs);
      });
  }

  private setupLineCalculations(): void {
    this.lineForm.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        const cost = Number(this.lineForm.get('cost')?.value ?? 0);
        const price = Number(this.lineForm.get('price')?.value ?? 0);
        const quantity = Number(this.lineForm.get('quantity')?.value ?? 0);
        const discountPercentage = Number(this.lineForm.get('discountPercentage')?.value ?? 0);

        const totals = calculateLineTotals(cost, price, quantity, discountPercentage);
        this.lineForm.patchValue({
          totalCost: totals.totalCost,
          totalSelling: totals.totalSelling
        }, { emitEvent: false });
      });
  }

  private setupItemAutocomplete(): void {
    this.lineForm.get('itemName')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        const term = (value ?? '').toString().trim().toLowerCase();
        if (!term) {
          this.filteredItems.set([]);
          return;
        }

        this.filteredItems.set(this.itemsMaster().filter((item) => item.name.toLowerCase().includes(term)));
      });
  }

  private populatePurchaseBill(purchaseBill: PurchaseBill): void {
    this.selectedBillId.set(purchaseBill.id);
    this.headerForm.patchValue({
      purchaseDate: purchaseBill.purchaseDate.slice(0, 10),
      supplierName: purchaseBill.supplierName,
      referenceNo: purchaseBill.referenceNo,
      notes: purchaseBill.notes
    });
    this.itemRows.set(purchaseBill.items.map((item) => ({ ...item })));
    this.resetLineForm();
    this.statusMessage.set(`Loaded ${purchaseBill.billNumber} for editing.`);
  }

  private validateAll(): boolean {
    const hasItems = this.itemRows().length > 0;
    if (this.headerForm.invalid || !hasItems) {
      this.headerForm.markAllAsTouched();
      if (!hasItems) {
        this.errorMessage.set('Add at least one line item before saving.');
      }
      return false;
    }

    this.errorMessage.set('');
    return true;
  }

  private buildPayload(syncStatus: string): SavePurchaseBillRequest {
    const headerValue = this.headerForm.getRawValue();
    const existingQueueEntry = this.pendingQueue().find((entry) => entry.purchaseBillId === this.selectedBillId());

    return {
      id: this.selectedBillId(),
      purchaseDate: headerValue.purchaseDate ?? this.todayString(),
      supplierName: (headerValue.supplierName ?? '').trim(),
      referenceNo: (headerValue.referenceNo ?? '').trim(),
      notes: (headerValue.notes ?? '').trim(),
      syncStatus,
      offlineClientId: existingQueueEntry?.payload.offlineClientId ?? null,
      items: this.itemRows()
    };
  }

  private resetLineForm(): void {
    this.lineForm.reset({
      itemName: '',
      batchCode: '',
      cost: 0,
      price: 0,
      quantity: 1,
      discountPercentage: 0,
      totalCost: 0,
      totalSelling: 0
    });
    this.filteredItems.set([]);
  }

  private todayString(): string {
    return new Date().toISOString().slice(0, 10);
  }

  private createOfflineId(): string {
    return `offline-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
  }
}
