using Enhanzer.Api.Entities;

namespace Enhanzer.Api.Interfaces;

public interface IPurchaseBillRepository
{
    Task<List<PurchaseBillHeader>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PurchaseBillHeader?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PurchaseBillHeader?> GetByOfflineClientIdAsync(string offlineClientId, CancellationToken cancellationToken = default);
    Task<int> GetNextSequenceAsync(CancellationToken cancellationToken = default);
    Task AddAsync(PurchaseBillHeader entity, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
