using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

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
    private readonly IDeviceQueryService _deviceService;
    private readonly ILogger<FleetStatusController> _logger;

    public FleetStatusController(
        IDeviceQueryService deviceService,
        ILogger<FleetStatusController> logger)
    {
        _deviceService = deviceService;
        _logger = logger;
    }

    /// <summary>Returns the most recent status event for every device.</summary>
    [HttpGet("status/latest")]
    public async Task<IActionResult> GetFleetStatusLatestAsync([FromQuery] string? tz, CancellationToken ct)
    {
        _logger.LogInformation("GetFleetStatusLatest — entry");

        IReadOnlyList<DeviceStatusItemDto> data;
        try
        {
            data = await _deviceService.GetLatestStatusAllDevicesAsync(tz, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFleetStatusLatest — DB read failed");
            return StatusCode(500, new ApiListResponse<DeviceStatusItemDto> { ReturnCode = 500 });
        }

        _logger.LogInformation("GetFleetStatusLatest — exit, {Count} devices", data.Count);
        return Ok(new ApiListResponse<DeviceStatusItemDto>
        {
            ReturnCode = 0,
            Count = data.Count,
            Data = data
        });
    }
}
