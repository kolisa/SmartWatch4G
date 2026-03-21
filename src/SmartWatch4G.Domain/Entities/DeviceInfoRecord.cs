namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// Persisted record of device registration / info reported by the wearable.
/// </summary>
public sealed class DeviceInfoRecord
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string Imsi { get; set; } = string.Empty;
    public string Sn { get; set; } = string.Empty;
    public string Mac { get; set; } = string.Empty;
    public string NetType { get; set; } = string.Empty;
    public string NetOperator { get; set; } = string.Empty;
    public string WearingStatus { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Sim1IccId { get; set; } = string.Empty;
    public string Sim1CellId { get; set; } = string.Empty;
    public string Sim1NetAdhere { get; set; } = string.Empty;
    public string NetworkStatus { get; set; } = string.Empty;
    public string BandDetail { get; set; } = string.Empty;
    public string RefSignal { get; set; } = string.Empty;
    public string Band { get; set; } = string.Empty;
    public string CommunicationMode { get; set; } = string.Empty;
    public int WatchEvent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
