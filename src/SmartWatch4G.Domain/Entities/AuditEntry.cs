namespace SmartWatch4G.Domain.Entities;

public class AuditEntry
{
    public long Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string? DeviceId { get; set; }
    public string? Details { get; set; }
    public DateTime OccurredAt { get; set; }
}
