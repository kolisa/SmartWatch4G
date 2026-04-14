using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SampleApi.Model;

namespace SampleApi.Controller {
    [Route("status/notify")]
    [ApiController]
    public class DeviceStatusController : ControllerBase {
        private readonly ILogger<DeviceStatusController> logger;

        public DeviceStatusController(ILogger<DeviceStatusController> thelogger){
            logger = thelogger;
        }

        [HttpPost]
        public async Task<IActionResult> NotifyDeviceStatus(){
            ResponseCode response;
            DeviceStatus requestData = new DeviceStatus();
            try{
                using var reader = new StreamReader(Request.Body);
                var bodyData = await reader.ReadToEndAsync();
                logger.LogInformation("NotifyDeviceStatus: {BodyData}", bodyData);

                requestData = JsonSerializer.Deserialize<DeviceStatus>(bodyData ?? string.Empty);
                if (requestData == null){
                    response = new ResponseCode { ReturnCode = 10002 };
                    return Ok(response);
                }
            }
            catch (Exception ex){
                logger.LogError("Error reading or deserializing request: {Message}", ex.Message);
                response = new ResponseCode { ReturnCode = 10002 };
                return Ok(response);
            }

            response = new ResponseCode { ReturnCode = 0 };
            return Ok(response);
        }

    }
}
