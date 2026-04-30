using Asp.Versioning;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Interfaces;

namespace KhoiWatchData.Api.Controllers;

[ApiVersionNeutral]
[Route("deviceinfo/upload")]
[ApiController]
public class DeviceInfoController : ControllerBase
{
    private readonly ILogger<DeviceInfoController> _logger;
    private readonly IDatabaseService _db;

    public DeviceInfoController(ILogger<DeviceInfoController> logger, IDatabaseService db)
    {
        _logger = logger;
        _db     = db;
    }

    [HttpPost]
    public async Task<IActionResult> UploadDeviceInfo()
    {
        DeviceInfo? requestData;
        string bodyData;
        try
        {
            using var reader = new StreamReader(Request.Body);
            bodyData = await reader.ReadToEndAsync();
            _logger.LogInformation("UploadDeviceInfo: {BodyData}", bodyData);

            requestData = JsonSerializer.Deserialize<DeviceInfo>(bodyData);
            if (requestData == null || string.IsNullOrEmpty(requestData.DeviceId))
                return Ok(new ResponseCode { ReturnCode = 10002 });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error reading or deserializing request: {Message}", ex.Message);
            return Ok(new ResponseCode { ReturnCode = 10002 });
        }

        string recordedAt = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        await _db.InsertDeviceInfo(
            requestData.DeviceId,
            recordedAt,
            requestData.Model,
            requestData.Version,
            requestData.WearingStatus,
            signal: null,
            bodyData);

        return Ok(new ResponseCode { ReturnCode = 0 });
    }
}

