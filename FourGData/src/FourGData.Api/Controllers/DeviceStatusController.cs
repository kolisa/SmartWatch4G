using System.Text.Json;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Receives JSON device-status notifications from wearable devices.
/// Route: POST /status/notify
/// </summary>
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
        DeviceStatusDto? dto;
        try
        {
            using var reader = new StreamReader(Request.Body);
            string body = await reader.ReadToEndAsync(ct).ConfigureAwait(false);
            _logger.LogInformation("NotifyDeviceStatus payload: {Body}", body);

            dto = JsonSerializer.Deserialize<DeviceStatusDto>(body);
            if (dto is null)
            {
                return Ok(new ResponseCodeDto { ReturnCode = 10002 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("DeviceStatus deserialise error: {Message}", ex.Message);
            return Ok(new ResponseCodeDto { ReturnCode = 10002 });
        }

        await _statusRepo.AddAsync(new DeviceStatusRecord
        {
            DeviceId = dto.DeviceId,
            EventTime = dto.EventTime,
            Status = dto.Status
        }, ct).ConfigureAwait(false);

        return Ok(new ResponseCodeDto { ReturnCode = 0 });
    }
}
