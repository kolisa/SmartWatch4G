using Google.Protobuf;
using SampleApi.Utils;

namespace SampleApi.Calculation {
    public class AfPreprocessor {
        private readonly ILogger<AfPreprocessor> logger;

        public AfPreprocessor(ILogger<AfPreprocessor> thelogger){
            logger = thelogger;
        }

        public void PrepareRriData(byte[] pbData){
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
                    case HisDataType.RriData:
                        var hisRriData = hisData.Rri;
                        if (hisRriData != null)
                        {
                            /*
                            not like ecg, rri is not one time measure, but
                            continuous value, you can calculate af result
                            of random time range, combine all rri value of
                            the time range for calculation
                            */
                            long seconds = hisRriData.TimeStamp.DateTime_.Seconds;
                            string timeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
                            var rawRriList = hisRriData.RawData;
                            var rriList = new List<long>();

                            foreach (var rawRri in rawRriList){
                                long value = (long)rawRri;
                                long fValue = (value >> 16) & 0x0000ffff;
                                long sValue = value & 0x0000ffff;
                                rriList.Add(fValue);
                                rriList.Add(sValue);
                            }

                            logger.LogInformation($"----{timeStr} count:{rriList.Count}");
                        }
                        break;
                }
            }
        }

    }
}
