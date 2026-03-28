namespace Enhanzer.Api.Entities;

public class PurchaseBillHeader
{
    public int Id { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string ReferenceNo { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string SyncStatus { get; set; } = "Synced";
    public string? OfflineClientId { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<PurchaseBillLine> Items { get; set; } = [];
}
