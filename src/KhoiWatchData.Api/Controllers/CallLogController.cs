using System.Text;
using System.Text.Json;
using KhoiWatchData.Api.Storage;
using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.DTOs;

namespace KhoiWatchData.Api.Controllers;

[Route("call_log/upload")]
[ApiController]
public class CallLogController : ControllerBase
{
    private readonly ILogger<CallLogController> _logger;
    private readonly RawDataFileStore _rawDataStore;

    public CallLogController(ILogger<CallLogController> logger, RawDataFileStore rawDataStore)
    {
        _logger       = logger;
        _rawDataStore = rawDataStore;
    }

    [HttpPost]
    public async Task<IActionResult> UploadCallLog()
    {
        string bodyData = string.Empty;
        try
        {
            using var reader = new StreamReader(Request.Body);
            bodyData = await reader.ReadToEndAsync();
            _logger.LogInformation("UploadCallLog: {BodyData}", bodyData);

            var requestData = JsonSerializer.Deserialize<DeviceCallLogs>(bodyData ?? string.Empty);
            if (requestData == null)
                return Ok(new ResponseCode { ReturnCode = 10002 });

            try
            {
                await _rawDataStore.SaveAsync(requestData.DeviceId ?? "unknown", "calllog",
                    Encoding.UTF8.GetBytes(bodyData!));
                _logger.LogInformation("[call_log/upload] Raw payload saved for device {DeviceId}", requestData.DeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[call_log/upload] Failed to save raw payload for device {DeviceId}", requestData.DeviceId);
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
