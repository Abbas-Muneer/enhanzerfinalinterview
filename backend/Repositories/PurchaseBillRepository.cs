using Enhanzer.Api.Data;
using Enhanzer.Api.Entities;
using Enhanzer.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Enhanzer.Api.Repositories;

public class PurchaseBillRepository(ApplicationDbContext dbContext) : IPurchaseBillRepository
{
    public Task<List<PurchaseBillHeader>> GetAllAsync(CancellationToken cancellationToken = default) =>
        dbContext.PurchaseBillHeaders
            .AsNoTracking()
            .Include(header => header.Items)
            .OrderByDescending(header => header.UpdatedAt)
            .ToListAsync(cancellationToken);

    public Task<PurchaseBillHeader?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.PurchaseBillHeaders
            .Include(header => header.Items.OrderBy(item => item.LineOrder))
            .FirstOrDefaultAsync(header => header.Id == id, cancellationToken);

    public Task<PurchaseBillHeader?> GetByOfflineClientIdAsync(string offlineClientId, CancellationToken cancellationToken = default) =>
        dbContext.PurchaseBillHeaders
            .Include(header => header.Items.OrderBy(item => item.LineOrder))
            .FirstOrDefaultAsync(header => header.OfflineClientId == offlineClientId, cancellationToken);

    public async Task<int> GetNextSequenceAsync(CancellationToken cancellationToken = default)
    {
        var currentCount = await dbContext.PurchaseBillHeaders.CountAsync(cancellationToken);
        return currentCount + 1;
    }

    public async Task AddAsync(PurchaseBillHeader entity, CancellationToken cancellationToken = default) =>
        await dbContext.PurchaseBillHeaders.AddAsync(entity, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
