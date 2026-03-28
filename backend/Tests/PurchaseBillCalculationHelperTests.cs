using Enhanzer.Api.Helpers;

namespace Enhanzer.Api.Tests;

public class PurchaseBillCalculationHelperTests
{
    [Fact]
    public void CalculationHelper_ReturnsExpectedFinancialValues()
    {
        Assert.Equal(50m, PurchaseBillCalculationHelper.CalculateMargin(100m, 150m));
        Assert.Equal(400m, PurchaseBillCalculationHelper.CalculateTotalCost(100m, 5m, 20m));
        Assert.Equal(750m, PurchaseBillCalculationHelper.CalculateTotalSelling(150m, 5m));
    }
}
