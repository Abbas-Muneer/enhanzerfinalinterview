using System.ComponentModel.DataAnnotations;

namespace Enhanzer.Api.DTOs;

public class SavePurchaseBillRequestDto
{
    public int? Id { get; set; }
    public string? BillNumber { get; set; }
    public string? OfflineClientId { get; set; }

    [Required]
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    [Required]
    public string SupplierName { get; set; } = string.Empty;

    public string ReferenceNo { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string SyncStatus { get; set; } = "Synced";

    [MinLength(1)]
    public List<PurchaseBillLineRequestDto> Items { get; set; } = [];
}
