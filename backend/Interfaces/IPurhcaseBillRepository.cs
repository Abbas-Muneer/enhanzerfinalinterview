using Enhanzer.Api.DTOs;

namespace Enhanzer.Api.Interfaces;

public interface IPurchaseBillService
{
    Task<IReadOnlyList<PurchaseBillListItemDto>> GetPurchaseBillsAsync(CancellationToken cancellationToken = default);
    Task<PurchaseBillDto?> GetPurchaseBillAsync(int id, CancellationToken cancellationToken = default);
    Task<PurchaseBillDto> CreatePurchaseBillAsync(SavePurchaseBillRequestDto request, CancellationToken cancellationToken = default);
    Task<PurchaseBillDto?> UpdatePurchaseBillAsync(int id, SavePurchaseBillRequestDto request, CancellationToken cancellationToken = default);
}
