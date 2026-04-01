using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Receives JSON device-status notifications from wearable devices.
/// Route: POST /status/notify
/// </summary>
[EnableRateLimiting("device-write")]
[Route("status/notify")]
[ApiController]
public sealed class DeviceStatusController : ControllerBase
{
    private readonly IDeviceStatusRepository _statusRepo;
    private readonly ILogger<DeviceStatusController> _logger;

    public DeviceStatusController(
        IDeviceStatusRepository statusRepo,
        ILogger<DeviceStatusController> logger)
    {
        _statusRepo = statusRepo;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> NotifyDeviceStatusAsync(CancellationToken ct)
    {
        _logger.LogInformation("NotifyDeviceStatus — entry from {RemoteIp}",
            HttpContext.Connection.RemoteIpAddress);

        DeviceStatusDto? dto;
        try
        {
            using var reader = new StreamReader(Request.Body);
            string body = await reader.ReadToEndAsync(ct).ConfigureAwait(false);
            _logger.LogInformation("NotifyDeviceStatus payload: {Body}", body);

            dto = JsonSerializer.Deserialize<DeviceStatusDto>(body);
            if (dto is null)
            {
                _logger.LogWarning("NotifyDeviceStatus — deserialization returned null");
                return Ok(new ResponseCodeDto { ReturnCode = 10002 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NotifyDeviceStatus — deserialise error");
            return Ok(new ResponseCodeDto { ReturnCode = 10002 });
        }

        if (string.IsNullOrWhiteSpace(dto.DeviceId))
        {
            _logger.LogWarning("NotifyDeviceStatus — DeviceId is empty, rejecting");
            return Ok(new ResponseCodeDto { ReturnCode = 10002 });
        }

        if (string.IsNullOrWhiteSpace(dto.EventTime))
        {
            _logger.LogWarning(
                "NotifyDeviceStatus — EventTime is empty for device {DeviceId}, rejecting",
                dto.DeviceId);
            return Ok(new ResponseCodeDto { ReturnCode = 10002 });
        }

        try
        {
            await _statusRepo.AddAsync(new DeviceStatusRecord
            {
                DeviceId = dto.DeviceId,
                EventTime = dto.EventTime,
                Status = dto.Status
            }, ct).ConfigureAwait(false);

            _logger.LogInformation(
                "NotifyDeviceStatus — exit, saved status for device {DeviceId}", dto.DeviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "NotifyDeviceStatus — DB write failed for device {DeviceId}", dto.DeviceId);
            return Ok(new ResponseCodeDto { ReturnCode = 10002 });
        }

        return Ok(new ResponseCodeDto { ReturnCode = 0 });
    }
}
