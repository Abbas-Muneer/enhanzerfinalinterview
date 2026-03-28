using Enhanzer.Api.DTOs;
using Enhanzer.Api.Entities;
using Enhanzer.Api.Interfaces;
using Enhanzer.Api.Services;
using Moq;

namespace Enhanzer.Api.Tests;

public class PurchaseBillServiceTests
{
    [Fact]
    public async Task CreatePurchaseBillAsync_CreatesAuditAndCalculatesTotals()
    {
        var purchaseBillRepository = new Mock<IPurchaseBillRepository>();
        var auditLogRepository = new Mock<IAuditLogRepository>();

        purchaseBillRepository.Setup(repository => repository.GetNextSequenceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        purchaseBillRepository.Setup(repository => repository.AddAsync(It.IsAny<PurchaseBillHeader>(), It.IsAny<CancellationToken>()))
            .Callback<PurchaseBillHeader, CancellationToken>((entity, _) => entity.Id = 10)
            .Returns(Task.CompletedTask);
        purchaseBillRepository.Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        auditLogRepository.Setup(repository => repository.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        auditLogRepository.Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new PurchaseBillService(purchaseBillRepository.Object, auditLogRepository.Object);

        var result = await service.CreatePurchaseBillAsync(new SavePurchaseBillRequestDto
        {
            SupplierName = "Fresh Fruits Supplier",
            PurchaseDate = new DateTime(2026, 3, 28),
            Items =
            [
                new PurchaseBillLineRequestDto
                {
                    ItemName = "Mango",
                    BatchCode = "LOC001",
                    Cost = 100,
                    Price = 150,
                    Quantity = 5,
                    DiscountPercentage = 20
                }
            ]
        });

        Assert.Equal(1, result.TotalItems);
        Assert.Equal(5, result.TotalQuantity);
        Assert.Equal(750, result.TotalAmount);
        Assert.Equal(400, result.TotalCost);
        auditLogRepository.Verify(repository => repository.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePurchaseBillAsync_WithDuplicateOfflineClientId_ReturnsExistingBill()
    {
        var purchaseBillRepository = new Mock<IPurchaseBillRepository>();
        var auditLogRepository = new Mock<IAuditLogRepository>();

        purchaseBillRepository.Setup(repository => repository.GetByOfflineClientIdAsync("offline-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PurchaseBillHeader
            {
                Id = 9,
                BillNumber = "PB-20260328-0001",
                SupplierName = "Existing Supplier",
                PurchaseDate = new DateTime(2026, 3, 28),
                SyncStatus = "Synced",
                TotalItems = 1,
                TotalQuantity = 2,
                TotalAmount = 300,
                TotalCost = 200,
                Items =
                [
                    new PurchaseBillLine
                    {
                        Id = 1,
                        ItemName = "Apple",
                        BatchCode = "LOC001",
                        Cost = 100,
                        Price = 150,
                        Quantity = 2,
                        DiscountPercentage = 0,
                        TotalCost = 200,
                        TotalSelling = 300,
                        LineOrder = 1
                    }
                ]
            });

        var service = new PurchaseBillService(purchaseBillRepository.Object, auditLogRepository.Object);

        var result = await service.CreatePurchaseBillAsync(new SavePurchaseBillRequestDto
        {
            SupplierName = "Ignored",
            PurchaseDate = new DateTime(2026, 3, 28),
            OfflineClientId = "offline-1",
            Items =
            [
                new PurchaseBillLineRequestDto
                {
                    ItemName = "Mango",
                    BatchCode = "LOC001",
                    Cost = 1,
                    Price = 1,
                    Quantity = 1,
                    DiscountPercentage = 0
                }
            ]
        });

        Assert.Equal(9, result.Id);
        purchaseBillRepository.Verify(repository => repository.AddAsync(It.IsAny<PurchaseBillHeader>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
