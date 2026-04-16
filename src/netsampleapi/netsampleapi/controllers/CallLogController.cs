using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SampleApi.Model;
using SampleApi.Storage;

namespace SampleApi.Controller {
    [Route("call_log/upload")]
    [ApiController]
    public class CallLogController : ControllerBase {
        private readonly ILogger<CallLogController> logger;
        private readonly RawDataFileStore rawDataStore;

        public CallLogController(ILogger<CallLogController> thelogger, RawDataFileStore theRawDataStore){
            logger = thelogger;
            rawDataStore = theRawDataStore;
        }

        [HttpPost]
        public async Task<IActionResult> UploadCallLog(){
            ResponseCode response;
            DeviceCallLogs requestData = new DeviceCallLogs();
            string bodyData = string.Empty;
            try{
                using var reader = new StreamReader(Request.Body);
                bodyData = await reader.ReadToEndAsync();
                logger.LogInformation("UploadCallLog: {BodyData}", bodyData);

                requestData = JsonSerializer.Deserialize<DeviceCallLogs>(bodyData ?? string.Empty);
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
                await rawDataStore.SaveAsync(requestData.DeviceId ?? "unknown", "calllog", Encoding.UTF8.GetBytes(bodyData!));
                logger.LogInformation("[call_log/upload] Raw payload saved for device {DeviceId}", requestData.DeviceId);
            }
            catch (Exception ex){
                logger.LogError(ex, "[call_log/upload] Failed to save raw payload for device {DeviceId}", requestData.DeviceId);
            }

            response = new ResponseCode { ReturnCode = 0 };
            return Ok(response);
        }

    }
}
