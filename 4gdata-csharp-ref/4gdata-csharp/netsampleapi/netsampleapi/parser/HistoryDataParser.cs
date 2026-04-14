using System;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using SampleApi.Utils;

namespace SampleApi.Parser {
    public class HistoryDataProcessor{
        private readonly ILogger<HistoryDataProcessor> logger;

        public HistoryDataProcessor(ILogger<HistoryDataProcessor> thelogger){
            logger = thelogger;
        }

        public void ProceedHistoryData(byte[] pbData){
            var hisNotify = new HisNotification();
            try{
                hisNotify = HisNotification.Parser.ParseFrom(pbData);
            }
            catch (InvalidProtocolBufferException e){
                logger.LogError($"Parse 80 health history error: {e.Message}");
                return;
            }
            if (hisNotify.DataCase == HisNotification.DataOneofCase.IndexTable){
                if(hisNotify.Type == HisDataType.YylpfeData){
                    var index_table = hisNotify.IndexTable;
                    var indexList = index_table.Index;
                    foreach (var index in indexList){
                        var startSeq = index.StartSeq;
                        var endSeq = index.EndSeq;
                        var time = index.Time;
                        logger.LogInformation($"startSeq: {startSeq}, endSeq: {endSeq}, time: {time}");
                    }
                }
            }
            else if (hisNotify.DataCase == HisNotification.DataOneofCase.HisData){
                var hisData = hisNotify.HisData;
                logger.LogInformation($"seq: {hisData.Seq}");
                switch (hisNotify.Type){
                    case HisDataType.HealthData:
                        var hisHealth = hisData.Health;
                        if (hisHealth != null){
                            // Handle Health Data
                            ParseHealthData(hisHealth);
                        }
                        break;
                    case HisDataType.EcgData:
                        var hisEcg = hisData.Ecg;
                        if (hisEcg != null)
                        {
                            // Handle ECG Data
                            ParseEcgData(hisEcg);
                        }
                        break;
                    case HisDataType.RriData:
                        var hisRri = hisData.Rri;
                        if (hisRri != null)
                        {
                            // Handle RRI Data
                            ParseRriData(hisRri);
                        }
                        break;
                    case HisDataType.Spo2Data:
                        var hisSpo2 = hisData.Spo2;
                        if (hisSpo2 != null)
                        {
                            // Handle SPO2 Data
                            ParseSpo2Data(hisSpo2);
                        }
                        break;
                    case HisDataType.ThirdpartyData:
                        var hisThirdParty = hisData.ThirdPartyData;
                        if (hisThirdParty != null)
                        {
                            // Handle Third Party Data
                            ParseThirdPartyData(hisThirdParty);
                        }
                        break;

                    case HisDataType.PpgData:
                        var hisPpg = hisData.Ppg;
                        if (hisPpg != null)
                        {
                            // Handle PPG Data
                            ParsePpgData(hisPpg);
                        }
                        break;
                    case HisDataType.AccelerometerData:
                        var hisAcc = hisData.ACCelerometerData;
                        if (hisAcc != null)
                        {
                            // Handle Accelerometer Data
                            ParseAccData(hisAcc);
                        }
                        break;
                    case HisDataType.MultiLeadsEcgData:
                        var hisMultiLeadsEcg = hisData.MultiLeadsECG;
                        if (hisMultiLeadsEcg != null)
                        {
                            // Handle Multi Leads ECG Data
                            ParseMultiLeadsEcgData(hisMultiLeadsEcg);
                        }
                        break;
                }
            }
        }

        public void ParseHealthData(HisDataHealth hisHealthData){
            long seconds = hisHealthData.TimeStamp.DateTime_.Seconds;
            string timeStr = MyDateTimeUtils.ParsePbDateTime(seconds);

            // Step data
            var pedoData = hisHealthData.PedoData;
            if (pedoData != null){
                uint step = pedoData.Step;
                float distance = pedoData.Distance * 0.1f;
                float calorie = pedoData.Calorie * 0.1f;
                logger.LogInformation($"----{timeStr} step:{step}, distance:{distance}, calorie:{calorie}");
            }

            // Heart rate data
            var hrData = hisHealthData.HrData;
            if (hrData != null){
                uint avgHr = hrData.AvgBpm;
                uint maxHr = hrData.MaxBpm;
                uint minHr = hrData.MinBpm;
                logger.LogInformation($"----{timeStr} avg hr:{avgHr}, max hr:{maxHr}, min hr:{minHr}");
            }

            // Spo2 data
            var boxyData = hisHealthData.BxoyData;
            if (boxyData != null){
                uint avgBoxy = boxyData.AgvOxy;
                uint maxBoxy = boxyData.MaxOxy;
                uint minBoxy = boxyData.MinOxy;
                logger.LogInformation($"----{timeStr} avg boxy:{avgBoxy}, max boxy:{maxBoxy}, min boxy:{minBoxy}");
            }

            // Blood pressure
            var bpData = hisHealthData.BpData;
            if (bpData != null){
                uint sbp = bpData.Sbp;
                uint dbp = bpData.Dbp;
                logger.LogInformation($"----{timeStr} sbp:{sbp}, dbp:{dbp}");
            }

            // HRV/Pressure
            var hrvData = hisHealthData.HrvData;
            if (hrvData != null){
                int fatigue = (int)hrvData.Fatigue;
                if (fatigue <= 0){
                    fatigue = (int)(Math.Log(hrvData.RMSSD) * 20);
                }
                logger.LogInformation($"----{timeStr} fatigue:{fatigue}");
            }

            // Temperature
            var tmprData = hisHealthData.TemperatureData;
            if (tmprData != null){
                float axillaryT = (tmprData.EstiArm & 0x0000ffff) / 100.0f;
                float estT = ((tmprData.EstiArm >> 16) & 0x0000ffff) / 100.0f;
                float shellT = (tmprData.EviBody & 0x0000ffff) / 100.0f;
                float envT = ((tmprData.EviBody >> 16) & 0x0000ffff) / 100.0f;
                logger.LogInformation($"----{timeStr} est_t:{estT}, shell_t:{shellT}, env_t:{envT}, axillary_t:{axillaryT}");
            }

            // Sleep data (not the final sleep result)
            var sleepData = hisHealthData.SleepData;
            if (sleepData != null){
                var sleepDataList = sleepData.SleepData;
                bool charge = sleepData.Charge;
                bool shutdown = sleepData.ShutDown;
                logger.LogInformation($"----{timeStr} charge:{charge}, shutdown:{shutdown}, count:{sleepDataList.Count}");
            }

            //bp hr
            var bpBpmData = hisHealthData.BpBpmData;
            if(bpBpmData != null){
                var bpm = bpBpmData.Bpm;
                logger.LogInformation($"----{timeStr} bp bpm:{bpm}");
            }

            //blood potassium
            var potassiumData = hisHealthData.BloodPotassiumData;
            if(potassiumData != null){
                var potassium = potassiumData.BloodPotassium;
                logger.LogInformation($"----{timeStr} potassium:{potassium}");
            }
            
            //bioz
            var biozData = hisHealthData.BiozData;
            if(biozData != null){
                var r = biozData.R;
                var x = biozData.X;
                var fat = biozData.Fat;
                var bmi = biozData.Bmi;
                var the_type = biozData.Type;
                logger.LogInformation($"----{timeStr} r:{r}, x:{x}, fat:{fat}, bmi:{bmi}, type:{the_type}");
            }

            //blood sugar
            var bloodSugarData = hisHealthData.BloodSugarData;
            if(bloodSugarData != null){
                var bloodSugar = bloodSugarData.BloodSugar;
                logger.LogInformation($"----{timeStr} bloodSugar:{bloodSugar}");
            }
        }

        public void ParseEcgData(HisDataECG hisEcgData){
            long seconds = hisEcgData.TimeStamp.DateTime_.Seconds;
            string timeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
            var rawEcgList = hisEcgData.RawData;
            logger.LogInformation($"----{timeStr} count:{rawEcgList.Count}");
        }

        public void ParseRriData(HisDataRRI hisRriData){
            long seconds = hisRriData.TimeStamp.DateTime_.Seconds;
            string timeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
            var rawRriList = hisRriData.RawData;
            var rriList = new List<short>();

            foreach (var rawRri in rawRriList){
                long value = (long)rawRri;
                short fValue = (short)((value >> 16) & 0x0000ffff);
                short sValue = (short)(value & 0x0000ffff);
                rriList.Add(fValue);
                rriList.Add(sValue);
            }

            logger.LogInformation($"----{timeStr} count:{rriList.Count}");
        }

        public void ParseSpo2Data(HisDataSpo2 hisSpo2Data){
            long seconds = hisSpo2Data.TimeStamp.DateTime_.Seconds;
            string timeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
            var rawSpo2List = hisSpo2Data.Spo2Data;

            foreach (var rawSpo2 in rawSpo2List){
                int spo2 = (int)((rawSpo2 >> 24) & 0xFF);
                int hr = (int)((rawSpo2 >> 16) & 0xFF);
                int perfusion = (int)((rawSpo2 >> 8) & 0xFF);
                int touch = (int)(rawSpo2 & 0xFF);

                logger.LogInformation($"----{timeStr} spo2:{spo2}, hr:{hr}, perfusion:{perfusion}, touch:{touch}");
            }
        }

        public void ParseThirdPartyData(HisDataThirdParty hisThirdPartyData){
            var thirdPartyHealth = hisThirdPartyData.DataHealth;
            if (thirdPartyHealth != null){
                var macAddr = thirdPartyHealth.MacAddr;
                logger.LogInformation($"----3rd party device mac address:{macAddr}");

                var bpData = thirdPartyHealth.BpData;
                if (bpData != null){
                    long seconds = bpData.Time.DateTime_.Seconds;
                    string timeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
                    uint sbp = bpData.Sbp;
                    uint dbp = bpData.Dbp;
                    uint hr = bpData.Hr;
                    uint pulse = bpData.Pulse;
                    MEASURE_MODE mode = bpData.MODE;
                    logger.LogInformation($"----3rd party {timeStr} sbp:{sbp}, dbp:{dbp}, hr:{hr}, pulse:{pulse}, mode:{mode}");
                }

                var gluData = thirdPartyHealth.GluData;
                if (gluData != null){
                    long seconds = gluData.Time.DateTime_.Seconds;
                    string timeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
                    uint bloodGlu = gluData.Glu;
                    logger.LogInformation($"----3rd party {timeStr} blood glucose:{bloodGlu}");
                }

                var scaleData = thirdPartyHealth.ScaleData;
                if (scaleData != null){
                    long seconds = scaleData.Time.DateTime_.Seconds;
                    string timeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
                    uint weight = scaleData.Weight;
                    uint impedance = scaleData.Impedance;
                    uint units = scaleData.Uints;
                    uint bodyFatPercentage = scaleData.BodyFatPercentage;
                    logger.LogInformation($"----3rd party {timeStr} weight:{weight}, impedance:{impedance}, units:{units}, body_fat_percentage:{bodyFatPercentage}");
                }

                var spo2Data = thirdPartyHealth.Spo2Data;
                if (spo2Data != null){
                    long seconds = spo2Data.Time.DateTime_.Seconds;
                    string timeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
                    uint spo2 = spo2Data.Spo2;
                    uint bpm = spo2Data.Bpm;
                    uint pi = spo2Data.Pi;
                    logger.LogInformation($"----3rd party {timeStr} spo2:{spo2}, hr:{bpm}, pi:{pi}");
                }

                var tmprData = thirdPartyHealth.TempData;
                if (tmprData != null){
                    long seconds = tmprData.Time.DateTime_.Seconds;
                    string timeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
                    uint tmpr = tmprData.BodyTemp;
                    logger.LogInformation($"----3rd party {timeStr} temperature:{tmpr}");
                }

                var bkData = thirdPartyHealth.BloodKetonesData;
                if (bkData != null){
                    long seconds = bkData.Time.DateTime_.Seconds;
                    string timeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
                    uint bkVal = bkData.BloodKetones;
                    logger.LogInformation($"----3rd party {timeStr} blood ketones:{bkVal}");
                }

                var uaData = thirdPartyHealth.UricAcidData;
                if (uaData != null){
                    long seconds = uaData.Time.DateTime_.Seconds;
                    string timeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
                    uint uaVal = uaData.UricAcid;
                    logger.LogInformation($"----3rd party {timeStr} uric acid:{uaVal}");
                }
            }
        }

        public void ParsePpgData(HisDataPPG hisPpgData){
            long seconds = hisPpgData.TimeStamp.DateTime_.Seconds;
            string timeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
            var rawPpgList = hisPpgData.RawData;
            var ppgList = new List<short>();

            foreach (var rawPpg in rawPpgList){
                int value = rawPpg;
                short fValue = (short)((value >> 16) & 0x0000FFFF);
                if (fValue < 0){
                    fValue = (short)(-fValue);
                }
                short sValue = (short)(value & 0x0000FFFF);
                if (sValue < 0){
                    sValue = (short)(-sValue);
                }
                ppgList.Add(fValue);
                ppgList.Add(sValue);
                logger.LogInformation($"{timeStr} ppg:{fValue},{sValue}");
            }
        }

        public void ParseAccData(HisACCelerometer hisAccData){
            long seconds = hisAccData.TimeStamp.DateTime_.Seconds;
            string timeStr = MyDateTimeUtils.ParsePbDateTime(seconds);

            var xList = ParseBytesString(hisAccData.AccX.ToByteArray());
            var yList = ParseBytesString(hisAccData.AccY.ToByteArray());
            var zList = ParseBytesString(hisAccData.AccZ.ToByteArray());

            for (int i = 0; i < xList.Count; i++){
                logger.LogInformation($"{timeStr} acc x:{xList[i]}, y:{yList[i]}, z:{zList[i]}");
            }
        }

        public void ParseMultiLeadsEcgData(HisDataMultiLeadsECG hisEcgData){
            long seconds = hisEcgData.TimeStamp.DateTime_.Seconds;
            string timeStr = MyDateTimeUtils.ParsePbDateTime(seconds);

            uint channel = hisEcgData.NumberOfChannels;
            uint singleChannelLength = hisEcgData.SingleDataByteLen;

            logger.LogInformation($"{timeStr} channel_num x:{channel}, single_data_len:{singleChannelLength}");

            var buffer = hisEcgData.RawData.ToByteArray();
            uint unitSize = channel * singleChannelLength;

            for (uint i = unitSize; i <= buffer.Length; i += unitSize){
                for (uint j = 0; j < channel; j++){
                    int num = 0;
                    for (uint k = 0; k < singleChannelLength; k++){
                        uint pos = i - singleChannelLength * (channel - j) + k;
                        int offset = (int)(8 * (singleChannelLength - k - 1));
                        int part = buffer[pos] << offset;
                        num += part;
                    }
                    logger.LogInformation($"channel {j + 1}, val: {num}");
                }
            }
        }

        public void ParseYylpfeData(HisDataYYLPFE hisYylData){
            uint dataTs = hisYylData.TimeStamp.DateTime_.Seconds;
            var data_bytes = hisYylData.RawData.ToByteArray();
            for(uint i = 0; i < data_bytes.Length-11; i += 12) {
                uint offsetTs = 0;
                ushort areaUp = 0, areaDown = 0, rri = 0, motion = 0;
                int step = 0;
                for (int k = 0; k < 4; k++)
                {
                    offsetTs |= (uint)((uint)data_bytes[i + step] << (8 * k));
                    step++;
                }

                for (int k = 0; k < 2; k++)
                {
                    areaUp |= (ushort)((ushort)data_bytes[i + step] << (8 * k));
                    step++;
                }

                for (int k = 0; k < 2; k++)
                {
                    areaDown |= (ushort)((ushort)data_bytes[i + step] << (8 * k));
                    step++;
                }

                for (int k = 0; k < 2; k++)
                {
                    rri |= (ushort)((ushort)data_bytes[i + step] << (8 * k));
                    step++;
                }

                for (int k = 0; k < 2; k++)
                {
                    motion |= (ushort)((ushort)data_bytes[i + step] << (8 * k));
                    step++;
                }
                uint rtcTs = dataTs + offsetTs / 1000;
                uint utcTs = rtcTs - (8 * 3600);
                logger.LogInformation($"ts:{utcTs}, area up:{areaUp}, area down:{areaDown}, rri:{rri}, motion:{motion}");
            }
        }

        private List<int> ParseBytesString(byte[] bytesArr){
            var vList = new List<int>();
            for (int i = 1; i < bytesArr.Length; i += 2){
                int low = bytesArr[i - 1];
                int high = bytesArr[i] << 8;
                int realVal = low + high;
                vList.Add(realVal);
            }
            return vList;
        }

    }
}

