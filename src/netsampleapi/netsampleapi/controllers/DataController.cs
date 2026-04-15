using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using SampleApi.Parser;
using SampleApi.Calculation;
using SampleApi.Storage;

namespace SampleApi.Controller {
    [Route("pb/upload")]
    [ApiController]
    public class DataController : ControllerBase {
        private readonly ILogger<DataController> logger;
        private readonly HistoryDataProcessor historyDataParser;
        private readonly OldManProcessor oldmanParser;
        private readonly AfPreprocessor af_preprocessor;
        private readonly EcgPreprocessor ecg_preprocessor;
        private readonly SleepPreprocessor sleep_preprocessor;
        private readonly RawDataFileStore rawDataStore;


        public DataController(ILogger<DataController> thelogger, 
            HistoryDataProcessor theHistoryParser,
            OldManProcessor theOldmanParser,
            AfPreprocessor theAfPreprocessor,
            EcgPreprocessor theEcgPreprocessor,
            SleepPreprocessor theSleepPreprocessor,
            RawDataFileStore theRawDataStore){
            logger = thelogger;
            historyDataParser = theHistoryParser;
            oldmanParser = theOldmanParser;
            af_preprocessor = theAfPreprocessor;
            ecg_preprocessor = theEcgPreprocessor;
            sleep_preprocessor = theSleepPreprocessor;
            rawDataStore = theRawDataStore;
        }

        [HttpPost]
        public async Task<ActionResult> UploadPbData(){
            byte[] payload;
            try{
                using var memoryStream = new MemoryStream();
                await Request.Body.CopyToAsync(memoryStream);
                payload = memoryStream.ToArray();
            }
            catch (Exception ex){
                logger.LogError("Read post data failed: {Message}", ex.Message);
                return File(new byte[] { 0x01 }, "text/plain");
            }
            
            if (payload.Length < 23){
                logger.LogWarning("Data length below 23");
                return File(new byte[] { 0x02 }, "text/plain");
            }

            string hexString = BitConverter.ToString(payload).Replace("-", "");
            logger.LogInformation("receive data: {hexString}",hexString);

            var deviceBytes = new byte[15];
            var prefixBytes = new byte[2];
            var lenBytes = new byte[2];
            var crcBytes = new byte[2];
            var optBytes = new byte[2];

            Array.Copy(payload, 0, deviceBytes, 0, 15);
            string device = Encoding.UTF8.GetString(deviceBytes);
            logger.LogInformation("Device: {Device}", device);

            // Persist the raw binary frame so it can be processed and saved to the database later
            await rawDataStore.SaveAsync(device, "pb", payload);

            int startPos = 15;
            while (true){
                if (payload.Length < startPos + 8){
                    logger.LogWarning("Data length below {Length}", startPos + 8);
                    return File(new byte[] { 0x02 }, "text/plain");
                }

                Array.Copy(payload, startPos, prefixBytes, 0, 2);
                if (prefixBytes[0] != 0x44 || prefixBytes[1] != 0x54){
                    logger.LogWarning("Invalid data header at {Position}", startPos);
                    return File(new byte[] { 0x03 }, "text/plain");
                }

                Array.Copy(payload, startPos + 2, lenBytes, 0, 2);
                int length = lenBytes[1] * 0x100 + lenBytes[0];

                Array.Copy(payload, startPos + 4, crcBytes, 0, 2);
                Array.Copy(payload, startPos + 6, optBytes, 0, 2);

                if (payload.Length < startPos + 8 + length){
                    logger.LogWarning("Data length below {Length}", startPos + 8 + length);
                    return File(new byte[] { 0x02 }, "text/plain");
                }

                var pbPayload = new byte[length];
                Array.Copy(payload, startPos + 8, pbPayload, 0, length);

                ushort opt = BitConverter.ToUInt16(optBytes, 0);

                switch (opt)
                {
                    case 0x0A:
                        oldmanParser.ProceedOldMan(pbPayload);
                        break;
                    case 0x80:
                        historyDataParser.ProceedHistoryData(pbPayload);
                        /*
                            this is for sleep calculation, show how to do some preprocess to prepare data
                            and then invoke the calculation api
                        */
                        sleep_preprocessor.PrepareSleepData(pbPayload);
                        /*
                            this is for ecg calculation, show how to do some preprocess to prepare data
                            and then invoke the calculation api
                        */
                        ecg_preprocessor.PrepareEcgData(pbPayload);
                        /*
                        this is for af calculation, show how to do some preprocess to prepare data
                        and then invoke the calculation api
                        */
                        af_preprocessor.PrepareRriData(pbPayload);
                        break;
                }
                startPos += 8 + length;
                if (payload.Length == startPos){
                    // Read to the end and no extra byte left
                    break;
                }
            }
            return File(new byte[] { 0x00 }, "text/plain");            
        }
    }
}
