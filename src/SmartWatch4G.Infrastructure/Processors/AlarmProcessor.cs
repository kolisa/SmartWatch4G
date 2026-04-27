using Google.Protobuf;
using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Interfaces;

namespace SmartWatch4G.Infrastructure.Processors;

public class AlarmProcessor
{
    private readonly ILogger<AlarmProcessor> _logger;
    private readonly IDatabaseService _db;

    public AlarmProcessor(ILogger<AlarmProcessor> logger, IDatabaseService db)
    {
        _logger = logger;
        _db     = db;
    }

    public async Task ProceedAlarmV2(byte[] pbData, string deviceId)
    {
        Alarm_infokConfirm alarmConfirm;
        try
        {
            alarmConfirm = Alarm_infokConfirm.Parser.ParseFrom(pbData);
        }
        catch (Exception ex)
        {
            _logger.LogError("Parse alarm v2 error: {Message}", ex.Message);
            return;
        }

        var pbAlarm1 = alarmConfirm.Alarm;
        if (pbAlarm1 != null)
        {
            foreach (var hrAlarm in pbAlarm1.AlarmHr)
            {
                var t = DateTimeUtilities.FromUnixSeconds(hrAlarm.TimeStamp.DateTime_.Seconds);
                _logger.LogInformation("----{Time} hr alarm, hr:{Hr}", t, hrAlarm.Hr);
                await _db.InsertAlarm(deviceId, t, "hr_alarm", $"hr:{hrAlarm.Hr}");
            }

            foreach (var spo2Alarm in pbAlarm1.AlarmSpo2)
            {
                var t = DateTimeUtilities.FromUnixSeconds(spo2Alarm.TimeStamp.DateTime_.Seconds);
                _logger.LogInformation("----{Time} spo2 alarm, boxy:{Spo2}", t, spo2Alarm.Spo2);
                await _db.InsertAlarm(deviceId, t, "spo2_alarm", $"spo2:{spo2Alarm.Spo2}");
            }

            foreach (var fallAlarm in pbAlarm1.AlarmFall)
            {
                var t = DateTimeUtilities.FromUnixSeconds(fallAlarm.TimeStamp.DateTime_.Seconds);
                _logger.LogInformation("----{Time} fall alarm", t);
                await _db.InsertAlarm(deviceId, t, "fall");
            }

            foreach (var tmprAlarm in pbAlarm1.AlarmTemperature)
            {
                var t = DateTimeUtilities.FromUnixSeconds(tmprAlarm.TimeStamp.DateTime_.Seconds);
                _logger.LogInformation("----{Time} temperature alarm, temperature:{Temp}", t, tmprAlarm.Temperature);
                await _db.InsertAlarm(deviceId, t, "temp_alarm", $"temp:{tmprAlarm.Temperature}");
            }

            foreach (var bpAlarm in pbAlarm1.AlarmBp)
            {
                var t = DateTimeUtilities.FromUnixSeconds(bpAlarm.TimeStamp.DateTime_.Seconds);
                _logger.LogInformation("----{Time} blood pressure alarm, sbp:{Sbp}, dbp:{Dbp}", t, bpAlarm.Sbp, bpAlarm.Dbp);
                await _db.InsertAlarm(deviceId, t, "bp_alarm", $"sbp:{bpAlarm.Sbp},dbp:{bpAlarm.Dbp}");
            }

            if (pbAlarm1.SOSNotificationTime != null)
            {
                var t = DateTimeUtilities.FromUnixSeconds(pbAlarm1.SOSNotificationTime.DateTime_.Seconds);
                _logger.LogInformation("---- sos alarm time {Time}", t);
                await _db.InsertAlarm(deviceId, t, "sos");
            }

            foreach (var potAlarm in pbAlarm1.AlarmBloodPotassium)
            {
                var t = DateTimeUtilities.FromUnixSeconds(potAlarm.TimeStamp.DateTime_.Seconds);
                _logger.LogInformation("----{Time} blood potassium alarm, potassium:{K}", t, potAlarm.BloodPotassium);
                await _db.InsertAlarm(deviceId, t, "blood_potassium", $"k:{potAlarm.BloodPotassium}");
            }

            foreach (var sugarAlarm in pbAlarm1.AlarmBloodSugar)
            {
                var t = DateTimeUtilities.FromUnixSeconds(sugarAlarm.TimeStamp.DateTime_.Seconds);
                _logger.LogInformation("----{Time} blood sugar alarm, blood sugar:{Sugar}", t, sugarAlarm.BloodSugar);
                await _db.InsertAlarm(deviceId, t, "blood_sugar", $"sugar:{sugarAlarm.BloodSugar}");
            }
        }

        var pbAlarm2 = alarmConfirm.Alarminfo;
        if (pbAlarm2 != null)
        {
            var t = DateTimeUtilities.FromUnixSeconds(pbAlarm2.TimeStamp.DateTime_.Seconds);

            if (pbAlarm2.HasLowpowerPercentage)
            {
                _logger.LogInformation("----{Time} low power alarm, battery:{Power}", t, pbAlarm2.LowpowerPercentage);
                await _db.InsertAlarm(deviceId, t, "low_power", $"battery:{pbAlarm2.LowpowerPercentage}");
            }

            if (pbAlarm2.HasPoweroffPercentage)
            {
                _logger.LogInformation("----{Time} power off alarm, battery:{Power}", t, pbAlarm2.PoweroffPercentage);
                await _db.InsertAlarm(deviceId, t, "power_off", $"battery:{pbAlarm2.PoweroffPercentage}");
            }

            if (pbAlarm2.HasWearstate)
            {
                _logger.LogInformation("----{Time} not wear alarm", t);
                await _db.InsertAlarm(deviceId, t, "not_wear");
            }

            if (pbAlarm2.HasInterceptNumber)
            {
                _logger.LogInformation("----{Time} phone intercept alarm, phone number:{Number}", t, pbAlarm2.InterceptNumber);
                await _db.InsertAlarm(deviceId, t, "phone_intercept", $"number:{pbAlarm2.InterceptNumber}");
            }
        }
    }
}
