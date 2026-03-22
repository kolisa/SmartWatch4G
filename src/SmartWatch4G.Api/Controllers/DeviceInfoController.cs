using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Receives JSON device-info uploads from wearable devices.
/// Route: POST /deviceinfo/upload
/// </summary>
[EnableRateLimiting("device-write")]
[Route("deviceinfo/upload")]
[ApiController]
public sealed class DeviceInfoController : ControllerBase
{
    private readonly IDeviceInfoRepository _deviceInfoRepo;
    private readonly ILogger<DeviceInfoController> _logger;

    public DeviceInfoController(
        IDeviceInfoRepository deviceInfoRepo,
        ILogger<DeviceInfoController> logger)
    {
        _deviceInfoRepo = deviceInfoRepo;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> UploadDeviceInfoAsync(CancellationToken ct)
    {
        _logger.LogInformation("UploadDeviceInfo — entry from {RemoteIp}",
            HttpContext.Connection.RemoteIpAddress);

        DeviceInfoDto? dto;
        try
        {
            using var reader = new StreamReader(Request.Body);
            string body = await reader.ReadToEndAsync(ct).ConfigureAwait(false);
            _logger.LogInformation("UploadDeviceInfo payload: {Body}", body);

            dto = JsonSerializer.Deserialize<DeviceInfoDto>(body);
            if (dto is null)
            {
                _logger.LogWarning("UploadDeviceInfo — deserialization returned null");
                return Ok(new ResponseCodeDto { ReturnCode = 10002 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UploadDeviceInfo — deserialise error");
            return Ok(new ResponseCodeDto { ReturnCode = 10002 });
        }

        if (string.IsNullOrWhiteSpace(dto.DeviceId))
        {
            _logger.LogWarning("UploadDeviceInfo — DeviceId is empty, rejecting");
            return Ok(new ResponseCodeDto { ReturnCode = 10002 });
        }

        try
        {
            await _deviceInfoRepo.UpsertAsync(new DeviceInfoRecord
            {
                DeviceId = dto.DeviceId,
                Imsi = dto.Imsi,
                Sn = dto.Sn,
                Mac = dto.Mac,
                NetType = dto.NetType,
                NetOperator = dto.NetOperator,
                WearingStatus = dto.WearingStatus,
                Model = dto.Model,
                Version = dto.Version,
                Sim1IccId = dto.Sim1IccId,
                Sim1CellId = dto.Sim1CellId,
                Sim1NetAdhere = dto.Sim1NetAdhere,
                NetworkStatus = dto.NetworkStatus,
                BandDetail = dto.BandDetail,
                RefSignal = dto.RefSignal,
                Band = dto.Band,
                CommunicationMode = dto.CommunicationMode,
                WatchEvent = dto.WatchEvent
            }, ct).ConfigureAwait(false);

            _logger.LogInformation(
                "UploadDeviceInfo — exit, upserted device {DeviceId}", dto.DeviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "UploadDeviceInfo — DB upsert failed for device {DeviceId}", dto.DeviceId);
            return Ok(new ResponseCodeDto { ReturnCode = 10002 });
        }

        return Ok(new ResponseCodeDto { ReturnCode = 0 });
    }
}
