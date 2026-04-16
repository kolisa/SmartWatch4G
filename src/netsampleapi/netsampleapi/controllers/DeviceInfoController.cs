using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SampleApi.Model;
using SampleApi.Storage;

namespace SampleApi.Controller {
    [Route("deviceinfo/upload")]
    [ApiController]
    public class DeviceInfoController : ControllerBase {
        private readonly ILogger<DeviceInfoController> logger;
        private readonly RawDataFileStore rawDataStore;

        public DeviceInfoController(ILogger<DeviceInfoController> thelogger, RawDataFileStore theRawDataStore){
            logger = thelogger;
            rawDataStore = theRawDataStore;
        }

        [HttpPost]
        public async Task<IActionResult> UploadDeviceInfo(){
            ResponseCode response;
            DeviceInfo requestData = new DeviceInfo();
            string bodyData = string.Empty;
            try{
                using var reader = new StreamReader(Request.Body);
                bodyData = await reader.ReadToEndAsync();
                logger.LogInformation("UploadDeviceInfo: {BodyData}", bodyData);

                requestData = JsonSerializer.Deserialize<DeviceInfo>(bodyData ?? string.Empty);
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

            // Persist the raw JSON so it can be saved to the database later
            try{
                await rawDataStore.SaveAsync(requestData.DeviceId ?? "unknown", "deviceinfo", Encoding.UTF8.GetBytes(bodyData!));
                logger.LogInformation("[deviceinfo/upload] Raw payload saved for device {DeviceId}", requestData.DeviceId);
            }
            catch (Exception ex){
                logger.LogError(ex, "[deviceinfo/upload] Failed to save raw payload for device {DeviceId}", requestData.DeviceId);
            }

            response = new ResponseCode { ReturnCode = 0 };
            return Ok(response);
        }

    }
}
