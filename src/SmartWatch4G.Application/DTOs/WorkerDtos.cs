namespace SmartWatch4G.Application.DTOs;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}

public sealed class WorkerSummaryResponse
{
    public string DeviceId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Surname { get; init; } = string.Empty;
    public string? EmpNo { get; init; }
    public double? LatestLatitude { get; init; }
    public double? LatestLongitude { get; init; }
    public string? LatestGnssTime { get; init; }
    public int? SpO2 { get; init; }
    public int? Steps { get; init; }
    public int? HeartRate { get; init; }
    public int? Fatigue { get; init; }
    public int? Battery { get; init; }
    public int? Sbp { get; init; }
    public int? Dbp { get; init; }
    public DateTime? HealthRecordedAt { get; init; }
}

public sealed class WorkerDetailResponse
{
    public string DeviceId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Surname { get; init; } = string.Empty;
    public string? EmpNo { get; init; }
    public string? Email { get; init; }
    public string? Cell { get; init; }
    public string? Address { get; init; }
    public string DeviceStatus { get; init; } = "unknown";
    public double? LatestLatitude { get; init; }
    public double? LatestLongitude { get; init; }
    public string? LatestGnssTime { get; init; }
    public int? SpO2 { get; init; }
    public int? Steps { get; init; }
    public int? HeartRate { get; init; }
    public int? MaxHeartRate { get; init; }
    public int? MinHeartRate { get; init; }
    public int? Fatigue { get; init; }
    public int? Battery { get; init; }
    public int? Sbp { get; init; }
    public int? Dbp { get; init; }
    public double? Distance { get; init; }
    public double? Calorie { get; init; }
    public DateTime? HealthRecordedAt { get; init; }
}
