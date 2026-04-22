using Google.Protobuf;
using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.Utilities;

namespace SmartWatch4G.Infrastructure.Processors;

public class AlarmProcessor
{
    private readonly ILogger<AlarmProcessor> _logger;

    public AlarmProcessor(ILogger<AlarmProcessor> logger)
    {
        _logger = logger;
    }

    public void ProceedAlarmV2(byte[] pbData)
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
                _logger.LogInformation("----{Time} hr alarm, hr:{Hr}",
                    DateTimeUtilities.FromUnixSeconds(hrAlarm.TimeStamp.DateTime_.Seconds), hrAlarm.Hr);

            foreach (var spo2Alarm in pbAlarm1.AlarmSpo2)
                _logger.LogInformation("----{Time} spo2 alarm, boxy:{Spo2}",
                    DateTimeUtilities.FromUnixSeconds(spo2Alarm.TimeStamp.DateTime_.Seconds), spo2Alarm.Spo2);

            foreach (var fallAlarm in pbAlarm1.AlarmFall)
                _logger.LogInformation("----{Time} fall alarm",
                    DateTimeUtilities.FromUnixSeconds(fallAlarm.TimeStamp.DateTime_.Seconds));

            foreach (var tmprAlarm in pbAlarm1.AlarmTemperature)
                _logger.LogInformation("----{Time} temperature alarm, temperature:{Temp}",
                    DateTimeUtilities.FromUnixSeconds(tmprAlarm.TimeStamp.DateTime_.Seconds), tmprAlarm.Temperature);

            foreach (var bpAlarm in pbAlarm1.AlarmBp)
                _logger.LogInformation("----{Time} blood pressure alarm, sbp:{Sbp}, dbp:{Dbp}",
                    DateTimeUtilities.FromUnixSeconds(bpAlarm.TimeStamp.DateTime_.Seconds), bpAlarm.Sbp, bpAlarm.Dbp);

            if (pbAlarm1.SOSNotificationTime != null)
                _logger.LogInformation("---- sos alarm time {Time}",
                    DateTimeUtilities.FromUnixSeconds(pbAlarm1.SOSNotificationTime.DateTime_.Seconds));

            foreach (var potAlarm in pbAlarm1.AlarmBloodPotassium)
                _logger.LogInformation("----{Time} blood potassium alarm, potassium:{K}",
                    DateTimeUtilities.FromUnixSeconds(potAlarm.TimeStamp.DateTime_.Seconds), potAlarm.BloodPotassium);

            foreach (var sugarAlarm in pbAlarm1.AlarmBloodSugar)
                _logger.LogInformation("----{Time} blood sugar alarm, blood sugar:{Sugar}",
                    DateTimeUtilities.FromUnixSeconds(sugarAlarm.TimeStamp.DateTime_.Seconds), sugarAlarm.BloodSugar);
        }

        var pbAlarm2 = alarmConfirm.Alarminfo;
        if (pbAlarm2 != null)
        {
            string alarmTimeStr = DateTimeUtilities.FromUnixSeconds(pbAlarm2.TimeStamp.DateTime_.Seconds);

            if (pbAlarm2.HasLowpowerPercentage)
                _logger.LogInformation("----{Time} low power alarm, battery:{Power}",
                    alarmTimeStr, pbAlarm2.LowpowerPercentage);

            if (pbAlarm2.HasPoweroffPercentage)
                _logger.LogInformation("----{Time} power off alarm, battery:{Power}",
                    alarmTimeStr, pbAlarm2.PoweroffPercentage);

            if (pbAlarm2.HasWearstate)
                _logger.LogInformation("----{Time} not wear alarm", alarmTimeStr);

            if (pbAlarm2.HasInterceptNumber)
                _logger.LogInformation("----{Time} phone intercept alarm, phone number:{Number}",
                    alarmTimeStr, pbAlarm2.InterceptNumber);
        }
    }
}
