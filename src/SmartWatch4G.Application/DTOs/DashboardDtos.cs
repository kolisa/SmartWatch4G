namespace SmartWatch4G.Application.DTOs;

public sealed class DashboardSummaryResponse
{
    public int TotalWorkers { get; init; }
    public int ActiveAlerts { get; init; }
    public int SosCount { get; init; }
    public int WorkersInDistress { get; init; }
}
