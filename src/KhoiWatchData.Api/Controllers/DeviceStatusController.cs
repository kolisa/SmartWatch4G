using System.Text;
using System.Text.Json;
using Asp.Versioning;
using KhoiWatchData.Api.Storage;
using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.DTOs;

namespace KhoiWatchData.Api.Controllers;

[ApiVersionNeutral]
[Route("status/notify")]
[ApiController]
public class DeviceStatusController : ControllerBase
{
    private readonly ILogger<DeviceStatusController> logger;

    public DeviceStatusController(ILogger<DeviceStatusController> thelogger)
    {
        logger = thelogger;
    }

    [HttpPost]
    public async Task<IActionResult> NotifyDeviceStatus()
    {
        ResponseCode response;
        DeviceStatus? requestData = null;
        try
        {
            using var reader = new StreamReader(Request.Body);
            var bodyData = await reader.ReadToEndAsync();
            logger.LogInformation("NotifyDeviceStatus: {BodyData}", bodyData);

            requestData = JsonSerializer.Deserialize<DeviceStatus>(bodyData ?? string.Empty);
            if (requestData == null)
            {
                response = new ResponseCode { ReturnCode = 10002 };
                return Ok(response);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error reading or deserializing request: {Message}", ex.Message);
            response = new ResponseCode { ReturnCode = 10002 };
            return Ok(response);
        }

        response = new ResponseCode { ReturnCode = 0 };
        return Ok(response);
    }

}
