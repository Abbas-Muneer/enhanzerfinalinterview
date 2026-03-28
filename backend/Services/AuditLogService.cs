using Enhanzer.Api.DTOs;
using Enhanzer.Api.Interfaces;

namespace Enhanzer.Api.Services;

public class AuditLogService(IAuditLogRepository auditLogRepository) : IAuditLogService
{
    public async Task<IReadOnlyList<AuditLogDto>> GetAuditLogsAsync(CancellationToken cancellationToken = default)
    {
        var logs = await auditLogRepository.GetAllAsync(cancellationToken);
        return logs.Select(log => new AuditLogDto
        {
            Id = log.Id,
            Entity = log.Entity,
            Action = log.Action,
            OldValue = log.OldValue,
            NewValue = log.NewValue,
            CreatedAt = log.CreatedAt
        }).ToList();
    }
}
