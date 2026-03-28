import { PurchaseBillLine } from '../../models/purchase-bill-line.model';

export function calculateLineTotals(cost: number, price: number, quantity: number, discountPercentage: number) {
  const totalCost = (cost * quantity) - ((cost * quantity) * discountPercentage / 100);
  const totalSelling = price * quantity;
  return {
    totalCost: Number.isFinite(totalCost) ? totalCost : 0,
    totalSelling: Number.isFinite(totalSelling) ? totalSelling : 0
  };
}

export function summarizePurchaseBill(items: PurchaseBillLine[]) {
  return {
    totalItems: items.length,
    totalQuantity: items.reduce((sum, item) => sum + item.quantity, 0),
    totalAmount: items.reduce((sum, item) => sum + item.totalSelling, 0),
    totalCost: items.reduce((sum, item) => sum + item.totalCost, 0)
  };
}
