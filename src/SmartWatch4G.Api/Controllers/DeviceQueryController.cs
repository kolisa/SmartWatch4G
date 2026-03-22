using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only device information endpoints consumed by mobile and web applications.
/// Routes:
///   GET /api/devices              — list all registered devices
///   GET /api/devices/{deviceId}   — single device detail
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/devices")]
public sealed class DeviceQueryController : ControllerBase
{
    private readonly IDeviceInfoRepository _deviceInfoRepo;
    private readonly ILogger<DeviceQueryController> _logger;

    public DeviceQueryController(
        IDeviceInfoRepository deviceInfoRepo,
        ILogger<DeviceQueryController> logger)
    {
        _deviceInfoRepo = deviceInfoRepo;
        _logger = logger;
    }

    /// <summary>Returns a summary list of all registered devices.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAllDevicesAsync([FromQuery] string? tz, CancellationToken ct)
    {
        _logger.LogInformation("GetAllDevices — entry");
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        IReadOnlyList<DeviceInfoRecord> records;
        try
        {
            records = await _deviceInfoRepo.GetAllAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllDevices — DB read failed");
            return StatusCode(500, new ApiListResponse<DeviceSummaryDto> { ReturnCode = 500 });
        }

        var data = records.Select(r => new DeviceSummaryDto
        {
            DeviceId = r.DeviceId,
            Model = r.Model,
            Version = r.Version,
            WearingStatus = r.WearingStatus,
            NetworkStatus = r.NetworkStatus,
            UpdatedAt = DateTimeUtilities.LocalizeDateTime(r.UpdatedAt, tzInfo)
        }).ToList();

        _logger.LogInformation("GetAllDevices — exit, {Count} devices", data.Count);
        return Ok(new ApiListResponse<DeviceSummaryDto>
        {
            ReturnCode = 0,
            Count = data.Count,
            Data = data
        });
    }

    /// <summary>Returns full detail for a single device.</summary>
    [HttpGet("{deviceId}")]
    public async Task<IActionResult> GetDeviceAsync(string deviceId, [FromQuery] string? tz, CancellationToken ct)
    {
        _logger.LogInformation("GetDevice — entry, device: {DeviceId}", deviceId);
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new ApiItemResponse<DeviceDetailDto> { ReturnCode = 400 });
        }

        DeviceInfoRecord? record;
        try
        {
            record = await _deviceInfoRepo.FindByDeviceIdAsync(deviceId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDevice — DB read failed for device {DeviceId}", deviceId);
            return StatusCode(500, new ApiItemResponse<DeviceDetailDto> { ReturnCode = 500 });
        }

        if (record is null)
        {
            _logger.LogInformation("GetDevice — not found: {DeviceId}", deviceId);
            return NotFound(new ApiItemResponse<DeviceDetailDto> { ReturnCode = 404 });
        }

        _logger.LogInformation("GetDevice — exit, device: {DeviceId}", deviceId);
        return Ok(new ApiItemResponse<DeviceDetailDto>
        {
            ReturnCode = 0,
            Data = new DeviceDetailDto
            {
                DeviceId = record.DeviceId,
                Imsi = record.Imsi,
                Sn = record.Sn,
                Mac = record.Mac,
                NetType = record.NetType,
                NetOperator = record.NetOperator,
                WearingStatus = record.WearingStatus,
                Model = record.Model,
                Version = record.Version,
                Sim1IccId = record.Sim1IccId,
                Sim1CellId = record.Sim1CellId,
                Sim1NetAdhere = record.Sim1NetAdhere,
                NetworkStatus = record.NetworkStatus,
                BandDetail = record.BandDetail,
                RefSignal = record.RefSignal,
                Band = record.Band,
                CommunicationMode = record.CommunicationMode,
                WatchEvent = record.WatchEvent,
                CreatedAt = DateTimeUtilities.LocalizeDateTime(record.CreatedAt, tzInfo),
                UpdatedAt = DateTimeUtilities.LocalizeDateTime(record.UpdatedAt, tzInfo)
            }
        });
    }
}
