using Enhanzer.Api.Entities;

namespace Enhanzer.Api.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog entity, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
