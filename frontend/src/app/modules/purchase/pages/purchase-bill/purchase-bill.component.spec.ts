import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { MasterDataService } from '../../../../core/services/master-data.service';
import { OfflineSyncService } from '../../../../services/offline-sync.service';
import { PurchaseBillService } from '../../../../services/purchase-bill.service';
import { PurchaseBillComponent } from './purchase-bill.component';

describe('PurchaseBillComponent', () => {
  let component: PurchaseBillComponent;
  let fixture: ComponentFixture<PurchaseBillComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PurchaseBillComponent],
      providers: [
        {
          provide: MasterDataService,
          useValue: {
            getItems: () => of([{ id: 1, name: 'Mango' }, { id: 2, name: 'Apple' }]),
            getLocations: () => of([{ id: 1, code: 'LOC001', name: 'Warehouse A' }])
          }
        },
        {
          provide: PurchaseBillService,
          useValue: {
            getPurchaseBills: () => of([]),
            getAuditLogs: () => of([]),
            getPurchaseBill: jasmine.createSpy('getPurchaseBill'),
            createPurchaseBill: jasmine.createSpy('createPurchaseBill'),
            updatePurchaseBill: jasmine.createSpy('updatePurchaseBill')
          }
        },
        {
          provide: OfflineSyncService,
          useValue: {
            getPendingQueue: () => [],
            enqueue: jasmine.createSpy('enqueue'),
            syncPending: () => of(0)
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PurchaseBillComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('calculates line totals in real time', () => {
    component['lineForm'].patchValue({
      cost: 100,
      price: 150,
      quantity: 5,
      discountPercentage: 20
    });

    expect(component['lineForm'].get('totalCost')?.value).toBe(400);
    expect(component['lineForm'].get('totalSelling')?.value).toBe(750);
  });

  it('adds rows and updates summary totals', () => {
    component['lineForm'].patchValue({
      itemName: 'Mango',
      batchCode: 'LOC001',
      cost: 100,
      price: 150,
      quantity: 5,
      discountPercentage: 20
    });

    component['addOrUpdateLine']();

    expect(component['itemRows']().length).toBe(1);
    expect(component['summary']().totalItems).toBe(1);
    expect(component['summary']().totalAmount).toBe(750);
  });
});
