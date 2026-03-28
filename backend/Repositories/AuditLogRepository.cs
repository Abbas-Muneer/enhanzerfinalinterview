using Enhanzer.Api.Data;
using Enhanzer.Api.Entities;
using Enhanzer.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Enhanzer.Api.Repositories;

public class AuditLogRepository(ApplicationDbContext dbContext) : IAuditLogRepository
{
    public Task<List<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default) =>
        dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(AuditLog entity, CancellationToken cancellationToken = default) =>
        await dbContext.AuditLogs.AddAsync(entity, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
