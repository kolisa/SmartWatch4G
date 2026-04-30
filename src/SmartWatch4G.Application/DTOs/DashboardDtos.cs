namespace SmartWatch4G.Application.DTOs;

public sealed class DashboardStatsResponse
{
    public int OnlineCount { get; init; }
    public int OfflineCount { get; init; }
    public int TotalCount { get; init; }
    public int WorkersInDistress { get; init; }
    public int HrAlertCount { get; init; }
    public int TrackedOnMap { get; init; }
}

// Keep for backward compat with FleetController
public sealed class DashboardSummaryResponse
{
    public int TotalWorkers { get; init; }
    public int ActiveAlerts { get; init; }
    public int SosCount { get; init; }
    public int WorkersInDistress { get; init; }
}
