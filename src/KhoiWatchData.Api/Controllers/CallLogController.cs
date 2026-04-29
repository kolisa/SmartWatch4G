using System.Text;
using System.Text.Json;
using Asp.Versioning;
using KhoiWatchData.Api.Storage;
using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.DTOs;

namespace KhoiWatchData.Api.Controllers;

[ApiVersionNeutral]
[Route("call_log/upload")]
[ApiController]
public class CallLogController : ControllerBase
{
    private readonly ILogger<CallLogController> logger;

    public CallLogController(ILogger<CallLogController> thelogger)
    {
        logger = thelogger;
    }

    [HttpPost]
    public async Task<IActionResult> UploadCallLog()
    {
        ResponseCode response;
        DeviceCallLogs? requestData = null;
        try
        {
            using var reader = new StreamReader(Request.Body);
            var bodyData = await reader.ReadToEndAsync();
            logger.LogInformation("UploadCallLog: {BodyData}", bodyData);

            requestData = JsonSerializer.Deserialize<DeviceCallLogs>(bodyData ?? string.Empty);
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
