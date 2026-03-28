using System.Text.Json;
using Enhanzer.Api.DTOs;
using Enhanzer.Api.Entities;
using Enhanzer.Api.Helpers;
using Enhanzer.Api.Interfaces;

namespace Enhanzer.Api.Services;

public class PurchaseBillService(
    IPurchaseBillRepository purchaseBillRepository,
    IAuditLogRepository auditLogRepository) : IPurchaseBillService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public async Task<IReadOnlyList<PurchaseBillListItemDto>> GetPurchaseBillsAsync(CancellationToken cancellationToken = default)
    {
        var purchaseBills = await purchaseBillRepository.GetAllAsync(cancellationToken);
        return purchaseBills.Select(header => new PurchaseBillListItemDto
        {
            Id = header.Id,
            BillNumber = header.BillNumber,
            PurchaseDate = header.PurchaseDate,
            SupplierName = header.SupplierName,
            SyncStatus = header.SyncStatus,
            TotalAmount = header.TotalAmount,
            UpdatedAt = header.UpdatedAt
        }).ToList();
    }

    public async Task<PurchaseBillDto?> GetPurchaseBillAsync(int id, CancellationToken cancellationToken = default)
    {
        var purchaseBill = await purchaseBillRepository.GetByIdAsync(id, cancellationToken);
        return purchaseBill is null ? null : MapToDto(purchaseBill);
    }

    public async Task<PurchaseBillDto> CreatePurchaseBillAsync(SavePurchaseBillRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(request.OfflineClientId))
        {
            var existing = await purchaseBillRepository.GetByOfflineClientIdAsync(request.OfflineClientId, cancellationToken);
            if (existing is not null)
            {
                return MapToDto(existing);
            }
        }

        var entity = new PurchaseBillHeader
        {
            BillNumber = string.IsNullOrWhiteSpace(request.BillNumber)
                ? await GenerateBillNumberAsync(cancellationToken)
                : request.BillNumber,
            PurchaseDate = request.PurchaseDate,
            SupplierName = request.SupplierName.Trim(),
            ReferenceNo = request.ReferenceNo.Trim(),
            Notes = request.Notes.Trim(),
            SyncStatus = string.IsNullOrWhiteSpace(request.SyncStatus) ? "Synced" : request.SyncStatus,
            OfflineClientId = string.IsNullOrWhiteSpace(request.OfflineClientId) ? null : request.OfflineClientId.Trim()
        };

        ApplyItems(entity, request.Items);

        await purchaseBillRepository.AddAsync(entity, cancellationToken);
        await purchaseBillRepository.SaveChangesAsync(cancellationToken);

        await WriteAuditLogAsync("PurchaseBill", "Create", null, entity, cancellationToken);
        return MapToDto(entity);
    }

    public async Task<PurchaseBillDto?> UpdatePurchaseBillAsync(int id, SavePurchaseBillRequestDto request, CancellationToken cancellationToken = default)
    {
        var entity = await purchaseBillRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var oldState = JsonSerializer.Serialize(MapToDto(entity), JsonOptions);

        entity.PurchaseDate = request.PurchaseDate;
        entity.SupplierName = request.SupplierName.Trim();
        entity.ReferenceNo = request.ReferenceNo.Trim();
        entity.Notes = request.Notes.Trim();
        entity.SyncStatus = string.IsNullOrWhiteSpace(request.SyncStatus) ? entity.SyncStatus : request.SyncStatus;
        entity.UpdatedAt = DateTime.UtcNow;

        entity.Items.Clear();
        ApplyItems(entity, request.Items);

        await purchaseBillRepository.SaveChangesAsync(cancellationToken);
        await WriteAuditLogAsync("PurchaseBill", "Update", oldState, entity, cancellationToken);

        return MapToDto(entity);
    }

    private async Task<string> GenerateBillNumberAsync(CancellationToken cancellationToken)
    {
        var nextSequence = await purchaseBillRepository.GetNextSequenceAsync(cancellationToken);
        return $"PB-{DateTime.UtcNow:yyyyMMdd}-{nextSequence:0000}";
    }

    private static void ApplyItems(PurchaseBillHeader entity, IEnumerable<PurchaseBillLineRequestDto> items)
    {
        var mappedItems = items.Select((item, index) =>
        {
            var totalCost = PurchaseBillCalculationHelper.CalculateTotalCost(item.Cost, item.Quantity, item.DiscountPercentage);
            var totalSelling = PurchaseBillCalculationHelper.CalculateTotalSelling(item.Price, item.Quantity);

            return new PurchaseBillLine
            {
                ItemName = item.ItemName.Trim(),
                BatchCode = item.BatchCode.Trim(),
                Cost = item.Cost,
                Price = item.Price,
                Quantity = item.Quantity,
                DiscountPercentage = item.DiscountPercentage,
                TotalCost = totalCost,
                TotalSelling = totalSelling,
                LineOrder = index + 1
            };
        }).ToList();

        foreach (var line in mappedItems)
        {
            entity.Items.Add(line);
        }

        entity.TotalItems = mappedItems.Count;
        entity.TotalQuantity = mappedItems.Sum(line => line.Quantity);
        entity.TotalCost = mappedItems.Sum(line => line.TotalCost);
        entity.TotalAmount = mappedItems.Sum(line => line.TotalSelling);
        entity.UpdatedAt = DateTime.UtcNow;
    }

    private async Task WriteAuditLogAsync(string entityName, string action, string? oldValue, PurchaseBillHeader entity, CancellationToken cancellationToken)
    {
        var auditLog = new AuditLog
        {
            Entity = entityName,
            Action = action,
            OldValue = oldValue,
            NewValue = JsonSerializer.Serialize(MapToDto(entity), JsonOptions),
            CreatedAt = DateTime.UtcNow
        };

        await auditLogRepository.AddAsync(auditLog, cancellationToken);
        await auditLogRepository.SaveChangesAsync(cancellationToken);
    }

    private static PurchaseBillDto MapToDto(PurchaseBillHeader entity) =>
        new()
        {
            Id = entity.Id,
            BillNumber = entity.BillNumber,
            PurchaseDate = entity.PurchaseDate,
            SupplierName = entity.SupplierName,
            ReferenceNo = entity.ReferenceNo,
            Notes = entity.Notes,
            SyncStatus = entity.SyncStatus,
            OfflineClientId = entity.OfflineClientId,
            TotalItems = entity.TotalItems,
            TotalQuantity = entity.TotalQuantity,
            TotalAmount = entity.TotalAmount,
            TotalCost = entity.TotalCost,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Items = entity.Items
                .OrderBy(item => item.LineOrder)
                .Select(item => new PurchaseBillLineDto
                {
                    Id = item.Id,
                    ItemName = item.ItemName,
                    BatchCode = item.BatchCode,
                    Cost = item.Cost,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    DiscountPercentage = item.DiscountPercentage,
                    TotalCost = item.TotalCost,
                    TotalSelling = item.TotalSelling
                }).ToList()
        };
}
