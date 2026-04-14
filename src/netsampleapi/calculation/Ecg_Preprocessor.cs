using Google.Protobuf;
using SampleApi.Utils;

namespace SampleApi.Calculation {
    public class EcgPreprocessor {
        private readonly ILogger<EcgPreprocessor> logger;

        public EcgPreprocessor(ILogger<EcgPreprocessor> thelogger){
            logger = thelogger;
        }

        public void PrepareEcgData(byte[] pbData){
            var hisNotify = new HisNotification();
            try{
                hisNotify = HisNotification.Parser.ParseFrom(pbData);
            }
            catch (InvalidProtocolBufferException e){
                logger.LogError($"Parse 80 health history error: {e.Message}");
                return;
            }

            if (hisNotify.DataCase == HisNotification.DataOneofCase.HisData){
                var hisData = hisNotify.HisData;
                logger.LogInformation($"seq: {hisData.Seq}");
                switch (hisNotify.Type){
                    case HisDataType.EcgData:
                        var hisEcgData = hisData.Ecg;
                        if (hisEcgData != null)
                        {
                            /*
                                save rawDataList with data_time, later combine all ecg data
                                with the same data_time, same data_time means it's the same
                                ecg measurement
                            */
                            long seconds = hisEcgData.TimeStamp.DateTime_.Seconds;
                            string timeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
                            var rawEcgList = hisEcgData.RawData;
                            logger.LogInformation($"----{timeStr} count:{rawEcgList.Count}");
                        }
                        break;
                }
            }
        }
    }
}
