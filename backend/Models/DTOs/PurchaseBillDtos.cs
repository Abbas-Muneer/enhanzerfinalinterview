namespace Enhanzer.Api.DTOs;

public class PurchaseBillDto
{
    public int Id { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string ReferenceNo { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string SyncStatus { get; set; } = string.Empty;
    public string? OfflineClientId { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<PurchaseBillLineDto> Items { get; set; } = [];
}
