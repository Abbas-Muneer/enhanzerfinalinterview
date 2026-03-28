namespace Enhanzer.Api.Entities;

public class PurchaseBillLine
{
    public int Id { get; set; }
    public int PurchaseBillHeaderId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string BatchCode { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalSelling { get; set; }
    public int LineOrder { get; set; }
    public PurchaseBillHeader? PurchaseBillHeader { get; set; }
}
