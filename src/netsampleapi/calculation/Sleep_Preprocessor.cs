using Google.Protobuf;
using SampleApi.Utils;

namespace SampleApi.Calculation {
    public class SleepPreprocessor {
        private readonly ILogger<SleepPreprocessor> logger;

        public SleepPreprocessor(ILogger<SleepPreprocessor> thelogger){
            logger = thelogger;
        }

        public void PrepareSleepData(byte[] pbData) {
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
                    case HisDataType.HealthData:
                        var hisHealthData = hisData.Health;
                        if (hisHealthData != null){
                            long seconds = hisHealthData.TimeStamp.DateTime_.Seconds;
                            string timeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
                            var sleepDict = new Dictionary<string, object>();
                            sleepDict["Q"] = hisData.Seq;
                            System.DateTime dateTime = System.DateTime.ParseExact(timeStr, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                            int hour = dateTime.Hour;
                            int minute = dateTime.Minute;
                            int[] tArr = new int[2];
                            tArr[0] = hour;
                            tArr[1] = minute;
                            sleepDict["T"] = tArr;

                            // Step data
                            var pedoData = hisHealthData.PedoData;
                            if (pedoData != null){
                                var detailDict = new Dictionary<string, object>();
                                detailDict["s"] = pedoData.Step;
                                detailDict["c"] = pedoData.Calorie;
                                detailDict["d"] = pedoData.Distance;
                                detailDict["t"] = pedoData.Type;
                                detailDict["a"] = pedoData.State & 15;
                                sleepDict["P"] = detailDict;
                            }

                            // Heart rate data
                            var hrData = hisHealthData.HrData;
                            if (hrData != null){
                                uint avgHr = hrData.AvgBpm;
                                uint maxHr = hrData.MaxBpm;
                                uint minHr = hrData.MinBpm;
                                var detailDict = new Dictionary<string, object>();
                                if(avgHr > 0){
                                    detailDict["a"] = avgHr;
                                }
                                if(maxHr > 0){
                                    detailDict["x"] = maxHr;
                                }
                                if(minHr > 0){
                                    detailDict["n"] = minHr;
                                }
                            }

                            // HRV/Pressure
                            var hrvData = hisHealthData.HrvData;
                            if (hrvData != null){
                                var detailDict = new Dictionary<string, object>();
                                if(hrvData.SDNN > 0){
                                    detailDict["s"] = hrvData.SDNN/10.0;
                                }
                                if(hrvData.RMSSD > 0){
                                    detailDict["r"] = hrvData.RMSSD/10.0;
                                }
                                if(hrvData.PNN50 > 0){
                                    detailDict["p"] = hrvData.PNN50/10.0;
                                }
                                if(hrvData.MEAN > 0){
                                    detailDict["m"] = hrvData.MEAN/10.0;
                                }
                                var fatigue = (int)hrvData.Fatigue;
                                if (fatigue > -1000){
                                    if (fatigue <= 0){
                                        fatigue = (int)(Math.Log((double)hrvData.RMSSD) * 20);
                                    }
                                    if (fatigue > 0){
                                        detailDict["f"] = fatigue;
                                    }
                                    if (detailDict.Count > 0){
                                        sleepDict["V"] = detailDict;
                                    }
                                }
                            }

                            // Sleep data (not the final sleep result)
                            var sleepData = hisHealthData.SleepData;
                            if (sleepData != null){
                                var detailDict = new Dictionary<string, object>();
                                detailDict["a"] = sleepData.SleepData;
                                if(sleepData.ShutDown){
                                    detailDict["s"] = sleepData.ShutDown;
                                }
                                if(sleepData.Charge){
                                    detailDict["c"] = sleepData.Charge;
                                }
                                sleepDict["E"] = detailDict;
                            }

                            /*
                                save sleep_str with date_str/seq, later to do sleep calculation,
                                need combine all day's sleep_str
                            */
                            string sleepStr;
                            try
                            {
                                sleepStr = System.Text.Json.JsonSerializer.Serialize(sleepDict);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex.Message);
                                return;
                            }
                            logger.LogInformation($"{timeStr} {hisData.Seq} {sleepStr}");
                        }
                        break;
                }
            }
        }
    }
}
