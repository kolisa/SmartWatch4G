namespace SmartWatch4G.Domain.Entities;

/// <summary>
/// User profile row enriched with the latest health snapshot and GPS track,
/// returned by the single-query paged listing endpoint.
/// </summary>
public sealed class UserProfileWithData
{
    // ── Profile ───────────────────────────────────────────────────────────────
    public string DeviceId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Cell { get; set; }
    public string? EmpNo { get; set; }
    public string? Address { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Latest health snapshot (nullable — device may not have sent data yet) ─
    public int? Battery { get; set; }
    public int? AvgHr { get; set; }
    public int? MaxHr { get; set; }
    public int? MinHr { get; set; }
    public int? AvgSpo2 { get; set; }
    public int? Sbp { get; set; }
    public int? Dbp { get; set; }
    public int? Steps { get; set; }
    public double? Distance { get; set; }
    public double? Calorie { get; set; }
    public int? Fatigue { get; set; }
    public DateTime? HealthAt { get; set; }

    // ── Latest GPS track (nullable) ───────────────────────────────────────────
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }
    public string? GnssTime { get; set; }
}
