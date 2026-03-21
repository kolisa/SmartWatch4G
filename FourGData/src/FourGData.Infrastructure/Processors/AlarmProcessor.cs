using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Google.Protobuf;
using Microsoft.Extensions.Logging;

namespace SmartWatch4G.Infrastructure.Processors;

/// <summary>
/// Parses opcode 0x12 alarm-v2 protobuf packets and persists each alarm event.
/// Replaces the original <c>AlarmProcessor</c> flat-class.
/// </summary>
public sealed class AlarmProcessor
{
    private readonly IAlarmRepository _alarmRepo;
    private readonly ILogger<AlarmProcessor> _logger;

    public AlarmProcessor(IAlarmRepository alarmRepo, ILogger<AlarmProcessor> logger)
    {
        _alarmRepo = alarmRepo;
        _logger = logger;
    }

    public async Task ProcessAsync(
        string deviceId,
        byte[] pbData,
        CancellationToken cancellationToken = default)
    {
        Alarm_infokConfirm alarmConfirm;
        try
        {
            alarmConfirm = Alarm_infokConfirm.Parser.ParseFrom(pbData);
        }
        catch (InvalidProtocolBufferException ex)
        {
            _logger.LogError("Parse alarm v2 error: {Message}", ex.Message);
            return;
        }

        var events = new List<AlarmEventRecord>();

        ParseAlarmBatch(deviceId, alarmConfirm, events);
        ParseAlarmInfo(deviceId, alarmConfirm, events);

        if (events.Count > 0)
        {
            await _alarmRepo.AddRangeAsync(events, cancellationToken).ConfigureAwait(false);
        }
    }

    // ── Alarm batch (pbAlarm1) ────────────────────────────────────────────────

    private void ParseAlarmBatch(
        string deviceId,
        Alarm_infokConfirm confirm,
        List<AlarmEventRecord> events)
    {
        var alarm = confirm.Alarm;
        if (alarm is null) return;

        foreach (var a in alarm.AlarmHr)
        {
            string t = DateTimeUtilities.FromUnixSeconds(a.TimeStamp.DateTime_.Seconds);
            _logger.LogInformation("{Time} — HR alarm, HR: {Hr}", t, a.Hr);
            events.Add(MakeEvent(deviceId, "HR", t, a.Hr));
        }

        foreach (var a in alarm.AlarmSpo2)
        {
            string t = DateTimeUtilities.FromUnixSeconds(a.TimeStamp.DateTime_.Seconds);
            _logger.LogInformation("{Time} — SPO2 alarm, SPO2: {S}", t, a.Spo2);
            events.Add(MakeEvent(deviceId, "SPO2", t, a.Spo2));
        }

        foreach (var a in alarm.AlarmThrombus)
        {
            string t = DateTimeUtilities.FromUnixSeconds(a.TimeStamp.DateTime_.Seconds);
            _logger.LogInformation("{Time} — thrombus alarm", t);
            events.Add(MakeEvent(deviceId, "THROMBUS", t));
        }

        foreach (var a in alarm.AlarmFall)
        {
            string t = DateTimeUtilities.FromUnixSeconds(a.TimeStamp.DateTime_.Seconds);
            _logger.LogInformation("{Time} — fall alarm", t);
            events.Add(MakeEvent(deviceId, "FALL", t));
        }

        foreach (var a in alarm.AlarmTemperature)
        {
            string t = DateTimeUtilities.FromUnixSeconds(a.TimeStamp.DateTime_.Seconds);
            _logger.LogInformation("{Time} — temperature alarm, temp: {T}", t, a.Temperature);
            events.Add(MakeEvent(deviceId, "TEMPERATURE", t, a.Temperature));
        }

        foreach (var a in alarm.AlarmBp)
        {
            string t = DateTimeUtilities.FromUnixSeconds(a.TimeStamp.DateTime_.Seconds);
            _logger.LogInformation("{Time} — BP alarm, {Sbp}/{Dbp}", t, a.Sbp, a.Dbp);
            events.Add(MakeEvent(deviceId, "BP", t, a.Sbp, a.Dbp));
        }

        if (alarm.SOSNotificationTime is not null)
        {
            string t = DateTimeUtilities.FromUnixSeconds(alarm.SOSNotificationTime.DateTime_.Seconds);
            _logger.LogInformation("{Time} — SOS alarm", t);
            events.Add(MakeEvent(deviceId, "SOS", t));
        }

        foreach (var a in alarm.AlarmBloodPotassium)
        {
            string t = DateTimeUtilities.FromUnixSeconds(a.TimeStamp.DateTime_.Seconds);
            _logger.LogInformation("{Time} — blood potassium alarm: {K}", t, a.BloodPotassium);
            events.Add(MakeEvent(deviceId, "BLOOD_POTASSIUM", t, (double)a.BloodPotassium));
        }

        foreach (var a in alarm.AlarmBloodSugar)
        {
            string t = DateTimeUtilities.FromUnixSeconds(a.TimeStamp.DateTime_.Seconds);
            _logger.LogInformation("{Time} — blood sugar alarm: {S}", t, a.BloodSugar);
            events.Add(MakeEvent(deviceId, "BLOOD_SUGAR", t, (double)a.BloodSugar));
        }
    }

    // ── Alarm info (pbAlarm2) ─────────────────────────────────────────────────

    private void ParseAlarmInfo(
        string deviceId,
        Alarm_infokConfirm confirm,
        List<AlarmEventRecord> events)
    {
        var info = confirm.Alarminfo;
        if (info is null) return;

        string t = DateTimeUtilities.FromUnixSeconds(info.TimeStamp.DateTime_.Seconds);

        if (info.HasLowpowerPercentage)
        {
            _logger.LogInformation("{Time} — low power alarm, battery: {B}%", t, info.LowpowerPercentage);
            events.Add(MakeEvent(deviceId, "LOW_POWER", t, info.LowpowerPercentage));
        }

        if (info.HasPoweroffPercentage)
        {
            _logger.LogInformation("{Time} — power-off alarm, battery: {B}%", t, info.PoweroffPercentage);
            events.Add(MakeEvent(deviceId, "POWER_OFF", t, info.PoweroffPercentage));
        }

        if (info.HasWearstate)
        {
            _logger.LogInformation("{Time} — not-wearing alarm", t);
            events.Add(MakeEvent(deviceId, "NOT_WEARING", t));
        }

        if (info.HasInterceptNumber)
        {
            _logger.LogInformation("{Time} — phone intercept alarm, number: {N}", t, info.InterceptNumber);
            events.Add(new AlarmEventRecord
            {
                DeviceId = deviceId,
                AlarmType = "PHONE_INTERCEPT",
                AlarmTime = t,
                Notes = info.InterceptNumber
            });
        }
    }

    // ── Factories ─────────────────────────────────────────────────────────────

    private static AlarmEventRecord MakeEvent(
        string deviceId,
        string type,
        string time,
        double? v1 = null,
        double? v2 = null) =>
        new()
        {
            DeviceId = deviceId,
            AlarmType = type,
            AlarmTime = time,
            Value1 = v1,
            Value2 = v2
        };

    private static AlarmEventRecord MakeEvent(
        string deviceId,
        string type,
        string time,
        uint v1) =>
        MakeEvent(deviceId, type, time, (double)v1);
}
