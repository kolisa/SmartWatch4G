using Asp.Versioning;
using System.Text;
using System.Text.Json;
using KhoiWatchData.Api.Storage;
using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.DTOs;

namespace KhoiWatchData.Api.Controllers;

[ApiVersionNeutral]
[Route("deviceinfo/upload")]
[ApiController]
public class DeviceInfoController : ControllerBase
{
    private readonly ILogger<DeviceInfoController> logger;

    public DeviceInfoController(ILogger<DeviceInfoController> thelogger)
    {
        logger = thelogger;
    }

    [HttpPost]
    public async Task<IActionResult> UploadDeviceInfo()
    {
        ResponseCode response;
        DeviceInfo? requestData = null;
        try
        {
            using var reader = new StreamReader(Request.Body);
            var bodyData = await reader.ReadToEndAsync();
            logger.LogInformation("UploadDeviceInfo: {BodyData}", bodyData);

            requestData = JsonSerializer.Deserialize<DeviceInfo>(bodyData ?? string.Empty);
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
