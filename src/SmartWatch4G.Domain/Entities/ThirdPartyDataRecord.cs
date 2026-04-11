using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// Stores data received from a third-party device (V1 protocol) paired with the wearable.
/// Each field is nullable; only the fields present in the packet are populated.
/// Supports: BP monitor, glucometer, scale, pulse-oximeter, thermometer, blood-ketone meter,
/// and uric-acid meter.
/// </summary>
public sealed class ThirdPartyDataRecord
{
    public int Id { get; set; }
    public string? DeviceId { get; set; }

    /// <summary>MAC address of the paired third-party device.</summary>
    public string? MacAddr { get; set; }

    /// <summary>Measurement timestamp.</summary>
    private string _dataTime = string.Empty;
    public string DataTime
    {
        get => _dataTime;
        set => _dataTime = Guard.NotNullOrWhiteSpace(value, nameof(DataTime));
    }

    // ── Blood pressure (BP monitor) ───────────────────────────────────────────
    public int? BpSbp { get; set; }
    public int? BpDbp { get; set; }
    public int? BpHr { get; set; }
    public int? BpPulse { get; set; }

    // ── Weight scale ──────────────────────────────────────────────────────────
    public float? ScaleWeight { get; set; }
    public float? ScaleImpedance { get; set; }
    public float? ScaleBodyFatPercentage { get; set; }

    // ── Pulse oximeter ────────────────────────────────────────────────────────
    public int? OximeterSpo2 { get; set; }
    public int? OximeterHr { get; set; }
    public float? OximeterPi { get; set; }

    // ── Thermometer ───────────────────────────────────────────────────────────
    public float? BodyTemp { get; set; }

    // ── Glucometer ────────────────────────────────────────────────────────────
    public float? BloodGlucose { get; set; }

    // ── Blood-ketone meter ────────────────────────────────────────────────────
    public float? BloodKetones { get; set; }

    // ── Uric-acid meter ───────────────────────────────────────────────────────
    public float? UricAcid { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
