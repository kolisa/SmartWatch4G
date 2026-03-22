using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Fleet device-status endpoint consumed by mobile and web applications.
/// Route: GET /api/fleet/status/latest
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/fleet")]
public sealed class FleetStatusController : ControllerBase
{
    private readonly IDeviceStatusRepository _statusRepo;
    private readonly ILogger<FleetStatusController> _logger;

    public FleetStatusController(
        IDeviceStatusRepository statusRepo,
        ILogger<FleetStatusController> logger)
    {
        _statusRepo = statusRepo;
        _logger = logger;
    }

    /// <summary>Returns the most recent status event for every device.</summary>
    [HttpGet("status/latest")]
    public async Task<IActionResult> GetFleetStatusLatestAsync([FromQuery] string? tz, CancellationToken ct)
    {
        _logger.LogInformation("GetFleetStatusLatest — entry");
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        IReadOnlyList<DeviceStatusRecord> records;
        try
        {
            records = await _statusRepo.GetLatestAllDevicesAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFleetStatusLatest — DB read failed");
            return StatusCode(500, new ApiListResponse<DeviceStatusItemDto> { ReturnCode = 500 });
        }

        var data = records.Select(r => new DeviceStatusItemDto
        {
            DeviceId   = r.DeviceId,
            EventTime  = DateTimeUtilities.LocalizeTimestamp(r.EventTime, tzInfo),
            Status     = r.Status,
            ReceivedAt = DateTimeUtilities.LocalizeDateTime(r.ReceivedAt, tzInfo)
        }).ToList();

        _logger.LogInformation("GetFleetStatusLatest — exit, {Count} devices", data.Count);
        return Ok(new ApiListResponse<DeviceStatusItemDto>
        {
            ReturnCode = 0,
            Count = data.Count,
            Data = data
        });
    }
}
