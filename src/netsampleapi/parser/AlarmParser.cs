using System;
using System.Collections.Generic;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using SampleApi.Utils;

namespace SampleApi.Parser {
    public class AlarmProcessor{
        private readonly ILogger<AlarmProcessor> logger;

        public AlarmProcessor(ILogger<AlarmProcessor> thelogger){
            logger = thelogger;
        }

        public void ProceedAlarmV2(byte[] pbData){
            var alarmConfirm = new Alarm_infokConfirm();
            try{
                alarmConfirm = Alarm_infokConfirm.Parser.ParseFrom(pbData);
            }
            catch (Exception ex){
                logger.LogError($"Parse alarm v2 error: {ex.Message}");
                return;
            }

            var pbAlarm1 = alarmConfirm.Alarm;
            if (pbAlarm1 != null){
                // HR Alarms
                var hrAlarmList = pbAlarm1.AlarmHr;
                if (hrAlarmList.Count > 0){
                    foreach (var hrAlarm in hrAlarmList){
                        long seconds = hrAlarm.TimeStamp.DateTime_.Seconds;
                        string alarmTimeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
                        uint hr = hrAlarm.Hr;
                        logger.LogInformation($"----{alarmTimeStr} hr alarm, hr:{hr}");
                    }
                }

                // SpO2 Alarms
                var spo2AlarmList = pbAlarm1.AlarmSpo2;
                if (spo2AlarmList.Count > 0){
                    foreach (var spo2Alarm in spo2AlarmList){
                        long seconds = spo2Alarm.TimeStamp.DateTime_.Seconds;
                        string alarmTimeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
                        uint boxy = spo2Alarm.Spo2;
                        logger.LogInformation($"----{alarmTimeStr} spo2 alarm, boxy:{boxy}");
                    }
                }

                // Thrombus Alarms
                var thrombusAlarmList = pbAlarm1.AlarmThrombus;
                if (thrombusAlarmList.Count > 0){
                    foreach (var thrombusAlarm in thrombusAlarmList){
                        long seconds = thrombusAlarm.TimeStamp.DateTime_.Seconds;
                        string alarmTimeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
                        logger.LogInformation($"----{alarmTimeStr} thrombus alarm");
                    }
                }

                // Fall Alarms
                var fallAlarmList = pbAlarm1.AlarmFall;
                if (fallAlarmList.Count > 0){
                    foreach (var fallAlarm in fallAlarmList){
                        long seconds = fallAlarm.TimeStamp.DateTime_.Seconds;
                        string alarmTimeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
                        logger.LogInformation($"----{alarmTimeStr} fall alarm");
                    }
                }

                // Temperature Alarms
                var tmprAlarmList = pbAlarm1.AlarmTemperature;
                if (tmprAlarmList.Count > 0){
                    foreach (var tmprAlarm in tmprAlarmList){
                        long seconds = tmprAlarm.TimeStamp.DateTime_.Seconds;
                        string alarmTimeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
                        uint tmpr = tmprAlarm.Temperature;
                        logger.LogInformation($"----{alarmTimeStr} temperature alarm, temperature:{tmpr}");
                    }
                }

                // Blood Pressure Alarms
                var bpAlarmList = pbAlarm1.AlarmBp;
                if (bpAlarmList.Count > 0){
                    foreach (var bpAlarm in bpAlarmList){
                        long seconds = bpAlarm.TimeStamp.DateTime_.Seconds;
                        string alarmTimeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
                        uint sbp = bpAlarm.Sbp;
                        uint dbp = bpAlarm.Dbp;
                        logger.LogInformation($"----{alarmTimeStr} blood pressure alarm, sbp:{sbp}, dbp:{dbp}");
                    }
                }

                //sos alarm, will send an alarm without location first, before do locate/call
                var sosAlarmTime = pbAlarm1.SOSNotificationTime;
                if(sosAlarmTime != null){
                    long seconds = sosAlarmTime.DateTime_.Seconds;
                    string alarmTimeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
                    logger.LogInformation($"---- sos alarm time {alarmTimeStr}");
                }

                // Blood Potassium alarm
                var potassiumAlarmList = pbAlarm1.AlarmBloodPotassium;
                if (potassiumAlarmList.Count > 0){
                    foreach (var potassiumAlarm in potassiumAlarmList){
                        long seconds = potassiumAlarm.TimeStamp.DateTime_.Seconds;
                        string alarmTimeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
                        float potassium = potassiumAlarm.BloodPotassium;
                        logger.LogInformation($"----{alarmTimeStr} blood potassium alarm, potassium:{potassium}");
                    }
                }

                // Blood Sugar alarm
                var sugarAlarmList = pbAlarm1.AlarmBloodSugar;
                if (sugarAlarmList.Count > 0){
                    foreach (var sugarAlarm in sugarAlarmList){
                        long seconds = sugarAlarm.TimeStamp.DateTime_.Seconds;
                        string alarmTimeStr = MyDateTimeUtils.ParsePbDateTime(seconds);
                        float sugar = sugarAlarm.BloodSugar;
                        logger.LogInformation($"----{alarmTimeStr} blood sugar alarm, blood sugar:{sugar}");
                    }
                }
            }

            var pbAlarm2 = alarmConfirm.Alarminfo;
            if (pbAlarm2 != null){
                long seconds = pbAlarm2.TimeStamp.DateTime_.Seconds;
                string alarmTimeStr = MyDateTimeUtils.ParsePbDateTime(seconds);

                if (pbAlarm2.HasLowpowerPercentage){
                    uint power = pbAlarm2.LowpowerPercentage;
                    logger.LogInformation($"----{alarmTimeStr} low power alarm, battery:{power}");
                }

                if (pbAlarm2.HasPoweroffPercentage){
                    uint power = pbAlarm2.PoweroffPercentage;
                    logger.LogInformation($"----{alarmTimeStr} power off alarm, battery:{power}");
                }

                if (pbAlarm2.HasWearstate){
                    logger.LogInformation($"----{alarmTimeStr} not wear alarm");
                }

                if (pbAlarm2.HasInterceptNumber){
                    string number = pbAlarm2.InterceptNumber;
                    logger.LogInformation($"----{alarmTimeStr} phone intercept alarm, phone number:{number}");
                }
            }
        }
    }
}
