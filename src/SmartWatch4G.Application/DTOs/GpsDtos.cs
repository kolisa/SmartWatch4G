using System.ComponentModel.DataAnnotations;

namespace SmartWatch4G.Application.DTOs;

// ── Query parameters ─────────────────────────────────────────────────────────

public sealed class GpsQueryParams
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1.")]
    public int Page { get; init; } = 1;

    [Range(1, 500, ErrorMessage = "PageSize must be between 1 and 500.")]
    public int PageSize { get; init; } = 50;

    public DateTime? From { get; init; }
    public DateTime? To   { get; init; }

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
