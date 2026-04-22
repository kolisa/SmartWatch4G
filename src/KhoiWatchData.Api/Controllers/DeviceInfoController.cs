using System.Text;
using System.Text.Json;
using KhoiWatchData.Api.Storage;
using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.DTOs;

namespace KhoiWatchData.Api.Controllers;

[Route("deviceinfo/upload")]
[ApiController]
public class DeviceInfoController : ControllerBase
{
    private readonly ILogger<DeviceInfoController> _logger;
    private readonly RawDataFileStore _rawDataStore;

    public DeviceInfoController(ILogger<DeviceInfoController> logger, RawDataFileStore rawDataStore)
    {
        _logger       = logger;
        _rawDataStore = rawDataStore;
    }

    [HttpPost]
    public async Task<IActionResult> UploadDeviceInfo()
    {
        string bodyData = string.Empty;
        try
        {
            using var reader = new StreamReader(Request.Body);
            bodyData = await reader.ReadToEndAsync();
            _logger.LogInformation("UploadDeviceInfo: {BodyData}", bodyData);

            var requestData = JsonSerializer.Deserialize<DeviceInfo>(bodyData ?? string.Empty);
            if (requestData == null)
                return Ok(new ResponseCode { ReturnCode = 10002 });

            try
            {
                await _rawDataStore.SaveAsync(requestData.DeviceId ?? "unknown", "deviceinfo",
                    Encoding.UTF8.GetBytes(bodyData!));
                _logger.LogInformation("[deviceinfo/upload] Raw payload saved for device {DeviceId}", requestData.DeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[deviceinfo/upload] Failed to save raw payload for device {DeviceId}", requestData.DeviceId);
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
