using System.ComponentModel.DataAnnotations;

namespace SmartWatch4G.Application.DTOs;

// ── Query parameters ─────────────────────────────────────────────────────────

public sealed class GpsQueryParams
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1.")]
    public int Page { get; init; } = 1;

    [Range(1, 500, ErrorMessage = "PageSize must be between 1 and 500.")]
    public int PageSize { get; init; } = 50;

    public DateTime? From { get; init; } = DateTime.Today;
    public DateTime? To   { get; init; } = DateTime.Today.AddDays(1).AddTicks(-1);

    /// <summary>Sort by "time" (default) or "device".</summary>
    public string SortBy { get; init; } = "time";

    /// <summary>"asc" or "desc" (default).</summary>
    public string SortDir { get; init; } = "desc";
}

// ── Response DTOs ─────────────────────────────────────────────────────────────

public sealed class GpsTrackResponse
{
    public string   DeviceId  { get; init; } = string.Empty;
    public string?  UserName  { get; init; }
    public string   GnssTime  { get; init; } = string.Empty;
    public double   Latitude  { get; init; }
    public double   Longitude { get; init; }
    public string?  LocType   { get; init; }
    public DateTime RecordedAt { get; init; }
}

public sealed class GpsPagedResult
{
    public IReadOnlyList<GpsTrackResponse> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page       { get; init; }
    public int PageSize   { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public int OnlineCount  { get; init; }
    public int OfflineCount { get; init; }
}

public sealed class DeviceGpsStatusResponse
{
    public string    DeviceId   { get; init; } = string.Empty;
    public string?   UserName   { get; init; }
    public string    Status     { get; init; } = string.Empty;
    public int       StatusCode { get; init; }
    public string?   GnssTime   { get; init; }
    public double?   Latitude   { get; init; }
    public double?   Longitude  { get; init; }
    public string?   LocType    { get; init; }
    public DateTime? RecordedAt { get; init; }
}

// ── Map view DTOs ─────────────────────────────────────────────────────────────

/// <summary>Single GPS point for a map track line.</summary>
public sealed class MapTrackPoint
{
    public string   GnssTime   { get; init; } = string.Empty;
    public double   Latitude   { get; init; }
    public double   Longitude  { get; init; }
    public string?  LocType    { get; init; }
    public DateTime RecordedAt { get; init; }
}

/// <summary>Latest health snapshot attached to a device's map entry.</summary>
public sealed class MapHealthSnapshot
{
    public string    RecordTime { get; init; } = string.Empty;
    public int?      Battery    { get; init; }
    public int?      HeartRate  { get; init; }
    public int?      MaxHr      { get; init; }
    public int?      MinHr      { get; init; }
    public int?      SpO2       { get; init; }
    public int?      Sbp        { get; init; }
    public int?      Dbp        { get; init; }
    public int?      Fatigue    { get; init; }
    public int?      Steps      { get; init; }
    public double?   Distance   { get; init; }
    public double?   Calorie    { get; init; }
    public DateTime  RecordedAt { get; init; }
}

/// <summary>
/// All GPS tracks for one device on a given day, plus the latest health snapshot.
/// Designed to feed a map view: plot <see cref="Tracks"/> as a polyline,
/// the first element (most recent) as the current-position marker, and
/// <see cref="LatestHealth"/> in the marker popup.
/// </summary>
public sealed class DeviceMapResponse
{
    public string   DeviceId     { get; init; } = string.Empty;
    public string?  UserName     { get; init; }
    public string?  EmpNo        { get; init; }
    public string   Status       { get; init; } = "offline";
    public int      StatusCode   { get; init; }
    public DateTime Date         { get; init; }
    public IReadOnlyList<MapTrackPoint> Tracks { get; init; } = [];
    public MapHealthSnapshot?           LatestHealth { get; init; }
}
