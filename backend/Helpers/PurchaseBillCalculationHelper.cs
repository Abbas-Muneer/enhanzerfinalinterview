namespace Enhanzer.Api.Helpers;

public static PurchaseBillCalculationHelper
{
    public static decimal CalculateMargin(decimal standardCost, decimal standardPrice) => standardPrice - standardCost;

    public static decimal CalculateTotalCost(decimal standardCost, decimal quantity, decimal discountPercentage)
    {
        var subtotalCost = standardCost * quantity;
        return subtotalCost - (subtotalCost * discountPercentage / 100m);

    }

    public static decimal CalculateTotalSelling(decimal standardPrice,  decimal quantity) => standardPrice * quantity;

}