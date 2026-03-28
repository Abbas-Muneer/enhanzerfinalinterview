namespace Enhanzer.Api.DTOs;

public class PurchaseBillLineDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string BatchCode { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalSelling { get; set; }
}
