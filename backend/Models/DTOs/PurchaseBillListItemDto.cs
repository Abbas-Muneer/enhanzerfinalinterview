namespace Enhanzer.Api.DTOs;

public class PurchaseBillListItemDto
{
    public int Id { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string SyncStatus { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime UpdatedAt { get; set; }
}
