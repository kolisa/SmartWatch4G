using System.ComponentModel.DataAnnotations;

namespace SmartWatch4G.Application.DTOs;

// ── Query parameters ─────────────────────────────────────────────────────────

public sealed class HealthQueryParams
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1.")]
    public int Page { get; init; } = 1;

    [Range(1, 200, ErrorMessage = "PageSize must be between 1 and 200.")]
    public int PageSize { get; init; } = 20;

    public DateTime? From { get; init; }
    public DateTime? To   { get; init; }

    /// <summary>Sort by "time" (default) or "device".</summary>
    public string SortBy { get; init; } = "time";

    /// <summary>"asc" or "desc" (default).</summary>
    public string SortDir { get; init; } = "desc";
}

// ── Response DTOs ─────────────────────────────────────────────────────────────

public sealed class HealthRecordResponse
{
    public string    DeviceId   { get; init; } = string.Empty;
    public string?   UserName   { get; init; }
    public string    RecordTime { get; init; } = string.Empty;
    public int?      Battery    { get; init; }
    public int?      Steps      { get; init; }
    public double?   Distance   { get; init; }
    public double?   Calorie    { get; init; }
    public int?      HeartRate  { get; init; }
    public int?      MaxHr      { get; init; }
    public int?      MinHr      { get; init; }
    public int?      SpO2       { get; init; }
    public int?      Sbp        { get; init; }
    public int?      Dbp        { get; init; }
    public int?      Fatigue    { get; init; }
    public DateTime  RecordedAt { get; init; }
}

public sealed class HealthSummaryResponse
{
    public string DeviceId  { get; init; } = string.Empty;
    public string? UserName { get; init; }
    public double? AvgHeartRate { get; init; }
    public double? AvgSpO2     { get; init; }
    public double? AvgFatigue  { get; init; }
    public int?    MaxHr       { get; init; }
    public int?    MinHr       { get; init; }
    public int?    TotalSteps  { get; init; }
    public int     RecordCount { get; init; }
}

public sealed class HealthPagedResult
{
    public IReadOnlyList<HealthRecordResponse> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page       { get; init; }
    public int PageSize   { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}
