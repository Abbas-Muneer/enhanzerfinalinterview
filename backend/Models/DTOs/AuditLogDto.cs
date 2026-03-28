namespace Enhanzer.Api.DTOs;

public class AuditLogDto
{
    public int Id { get; set; }
    public string Entity { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedAt { get; set; }
}
