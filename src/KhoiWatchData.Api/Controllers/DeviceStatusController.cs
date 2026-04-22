using System.Text;
using System.Text.Json;
using KhoiWatchData.Api.Storage;
using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.DTOs;

namespace KhoiWatchData.Api.Controllers;

[Route("status/notify")]
[ApiController]
public class DeviceStatusController : ControllerBase
{
    private readonly ILogger<DeviceStatusController> _logger;
    private readonly RawDataFileStore _rawDataStore;

    public DeviceStatusController(ILogger<DeviceStatusController> logger, RawDataFileStore rawDataStore)
    {
        _logger       = logger;
        _rawDataStore = rawDataStore;
    }

    [HttpPost]
    public async Task<IActionResult> NotifyDeviceStatus()
    {
        string bodyData = string.Empty;
        try
        {
            using var reader = new StreamReader(Request.Body);
            bodyData = await reader.ReadToEndAsync();
            _logger.LogInformation("NotifyDeviceStatus: {BodyData}", bodyData);

            var requestData = JsonSerializer.Deserialize<DeviceStatus>(bodyData ?? string.Empty);
            if (requestData == null)
                return Ok(new ResponseCode { ReturnCode = 10002 });

            try
            {
                await _rawDataStore.SaveAsync(requestData.DeviceId ?? "unknown", "status",
                    Encoding.UTF8.GetBytes(bodyData!));
                _logger.LogInformation("[status/notify] Raw payload saved for device {DeviceId}", requestData.DeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[status/notify] Failed to save raw payload for device {DeviceId}", requestData.DeviceId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error reading or deserializing request: {Message}", ex.Message);
            return Ok(new ResponseCode { ReturnCode = 10002 });
        }

        return Ok(new ResponseCode { ReturnCode = 0 });
    }
}
