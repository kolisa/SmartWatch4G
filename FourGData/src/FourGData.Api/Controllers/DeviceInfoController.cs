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
        DeviceInfoDto? dto;
        try
        {
            using var reader = new StreamReader(Request.Body);
            string body = await reader.ReadToEndAsync(ct).ConfigureAwait(false);
            _logger.LogInformation("UploadDeviceInfo payload: {Body}", body);

            dto = JsonSerializer.Deserialize<DeviceInfoDto>(body);
            if (dto is null)
            {
                return Ok(new ResponseCodeDto { ReturnCode = 10002 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("DeviceInfo deserialise error: {Message}", ex.Message);
            return Ok(new ResponseCodeDto { ReturnCode = 10002 });
        }

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

        return Ok(new ResponseCodeDto { ReturnCode = 0 });
    }
}
