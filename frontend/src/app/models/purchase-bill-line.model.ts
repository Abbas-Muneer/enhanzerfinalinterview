export interface PurchaseBillLine {
  id?: number;
  itemName: string;
  batchCode: string;
  cost: number;
  price: number;
  quantity: number;
  discountPercentage: number;
  totalCost: number;
  totalSelling: number;
}
