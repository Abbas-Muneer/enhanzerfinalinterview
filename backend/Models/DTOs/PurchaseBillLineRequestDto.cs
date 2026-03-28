using System.ComponentModel.DataAnnotations;

namespace Enhanzer.Api.DTOs;

public class PurchaseBillLineRequestDto
{
    [Required]
    public string ItemName { get; set; } = string.Empty;

    [Required]
    public string BatchCode { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Cost { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Quantity { get; set; }

    [Range(0, 100)]
    public decimal DiscountPercentage { get; set; }
}