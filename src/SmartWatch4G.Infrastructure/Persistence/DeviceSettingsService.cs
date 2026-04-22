using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

namespace SmartWatch4G.Infrastructure.Persistence;

public class DeviceSettingsService : IDeviceSettingsService
{
    private readonly string _connStr;
    private readonly ILogger<DeviceSettingsService> _logger;

    public DeviceSettingsService(IConfiguration config, ILogger<DeviceSettingsService> logger)
    {
        _logger  = logger;
        _connStr = config.GetConnectionString("SmartWatch")
            ?? throw new InvalidOperationException("Connection string 'SmartWatch' not found.");
    }

    private SqlConnection Open() { var c = new SqlConnection(_connStr); c.Open(); return c; }

    private void Exec(string sql, Action<SqlCommand> bind, string ctx)
    {
        try
        {
            using var conn = Open();
            using var cmd  = new SqlCommand(sql, conn);
            bind(cmd);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { _logger.LogError(ex, "{Ctx} failed", ctx); }
    }

    // ── single-row-per-device helpers ──────────────────────────────────────────

    public void SaveUserInfo(UserInfoRequest r) => Exec(@"
        MERGE device_user_info AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET
            height=@h, weight=@w, gender=@g, age=@a,
            calibrate_walk=@cw, calibrate_run=@cr, wrist_circle=@wc, hypertension=@hp,
            updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT
            (device_id,height,weight,gender,age,calibrate_walk,calibrate_run,wrist_circle,hypertension)
            VALUES(@dev,@h,@w,@g,@a,@cw,@cr,@wc,@hp);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@h", r.height);
               c.Parameters.AddWithValue("@w", r.weight); c.Parameters.AddWithValue("@g", r.gender);
               c.Parameters.AddWithValue("@a", r.age); c.Parameters.AddWithValue("@cw", r.calibrate_walk);
               c.Parameters.AddWithValue("@cr", r.calibrate_run); c.Parameters.AddWithValue("@wc", r.wrist_circle);
               c.Parameters.AddWithValue("@hp", (object?)r.hypertension ?? DBNull.Value); },
        "SaveUserInfo");

    public void SaveFallCheck(FallCheckRequest r) => Exec(@"
        MERGE device_fall_settings AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET fall_check=@fc, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT (device_id,fall_check) VALUES(@dev,@fc);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@fc", r.fall_check); },
        "SaveFallCheck");

    public void SaveFallCheckSensitivity(FallCheckSensitivityRequest r) => Exec(@"
        MERGE device_fall_settings AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET fall_threshold=@ft, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT (device_id,fall_threshold) VALUES(@dev,@ft);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@ft", r.fall_threshold); },
        "SaveFallCheckSensitivity");

    public void SaveDataFreq(DataFreqRequest r) => Exec(@"
        MERGE device_data_freq AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET
            gps_auto_check=@gac, gps_interval_time=@git, power_mode=@pm, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT
            (device_id,gps_auto_check,gps_interval_time,power_mode)
            VALUES(@dev,@gac,@git,@pm);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@gac", r.gps_auto_check);
               c.Parameters.AddWithValue("@git", r.gps_interval_time);
               c.Parameters.AddWithValue("@pm", (object?)r.power_mode ?? DBNull.Value); },
        "SaveDataFreq");

    public void SaveLocateDataUploadFreq(LocateDataUploadFreqRequest r) => Exec(@"
        MERGE device_locate_freq AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET
            data_auto_upload=@dau, data_upload_interval=@dui, auto_locate=@al,
            locate_interval_time=@lit, power_mode=@pm, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT
            (device_id,data_auto_upload,data_upload_interval,auto_locate,locate_interval_time,power_mode)
            VALUES(@dev,@dau,@dui,@al,@lit,@pm);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@dau", r.data_auto_upload);
               c.Parameters.AddWithValue("@dui", r.data_upload_interval); c.Parameters.AddWithValue("@al", r.auto_locate);
               c.Parameters.AddWithValue("@lit", r.locate_interval_time);
               c.Parameters.AddWithValue("@pm", (object?)r.power_mode ?? DBNull.Value); },
        "SaveLocateDataUploadFreq");

    public void SaveLcdGesture(LcdGestureRequest r) => Exec(@"
        MERGE device_lcd_gesture AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET open=@o, start_hour=@sh, end_hour=@eh, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT (device_id,open,start_hour,end_hour) VALUES(@dev,@o,@sh,@eh);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@o", r.open);
               c.Parameters.AddWithValue("@sh", r.start_hour); c.Parameters.AddWithValue("@eh", r.end_hour); },
        "SaveLcdGesture");

    public void SaveHrAlarm(HrAlarmRequest r) => Exec(@"
        MERGE device_hr_alarm AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET
            open=@o, high=@h, low=@l, threshold=@th, alarm_interval=@ai, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT
            (device_id,open,high,low,threshold,alarm_interval)
            VALUES(@dev,@o,@h,@l,@th,@ai);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@o", r.open);
               c.Parameters.AddWithValue("@h", r.high); c.Parameters.AddWithValue("@l", r.low);
               c.Parameters.AddWithValue("@th", r.threshold); c.Parameters.AddWithValue("@ai", r.alarm_interval); },
        "SaveHrAlarm");

    public void SaveDynamicHrAlarm(DynamicHrAlarmRequest r) => Exec(@"
        MERGE device_dynamic_hr_alarm AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET
            open=@o, high=@h, low=@l, timeout=@to, interval=@iv, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT
            (device_id,open,high,low,timeout,interval)
            VALUES(@dev,@o,@h,@l,@to,@iv);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@o", r.open);
               c.Parameters.AddWithValue("@h", r.high); c.Parameters.AddWithValue("@l", r.low);
               c.Parameters.AddWithValue("@to", r.timeout); c.Parameters.AddWithValue("@iv", r.interval); },
        "SaveDynamicHrAlarm");

    public void SaveSpo2Alarm(Spo2AlarmRequest r) => Exec(@"
        MERGE device_spo2_alarm AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET open=@o, low=@l, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT (device_id,open,low) VALUES(@dev,@o,@l);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@o", r.open);
               c.Parameters.AddWithValue("@l", r.low); },
        "SaveSpo2Alarm");

    public void SaveBpAlarm(BpAlarmRequest r) => Exec(@"
        MERGE device_bp_alarm AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET
            open=@o, sbp_high=@sh, sbp_below=@sb, dbp_high=@dh, dbp_below=@db, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT
            (device_id,open,sbp_high,sbp_below,dbp_high,dbp_below)
            VALUES(@dev,@o,@sh,@sb,@dh,@db);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@o", r.open);
               c.Parameters.AddWithValue("@sh", r.sbp_high); c.Parameters.AddWithValue("@sb", r.sbp_below);
               c.Parameters.AddWithValue("@dh", r.dbp_high); c.Parameters.AddWithValue("@db", r.dbp_below); },
        "SaveBpAlarm");

    public void SaveTemperatureAlarm(TemperatureAlarmRequest r) => Exec(@"
        MERGE device_temp_alarm AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET open=@o, high=@h, low=@l, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT (device_id,open,high,low) VALUES(@dev,@o,@h,@l);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@o", r.open);
               c.Parameters.AddWithValue("@h", r.high); c.Parameters.AddWithValue("@l", r.low); },
        "SaveTemperatureAlarm");

    public void SaveAutoAf(AutoAfRequest r) => Exec(@"
        MERGE device_auto_af AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET
            open=@o, interval=@iv, rri_single_time=@rst, rri_type=@rt, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT
            (device_id,open,interval,rri_single_time,rri_type)
            VALUES(@dev,@o,@iv,@rst,@rt);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@o", r.open);
               c.Parameters.AddWithValue("@iv", r.interval);
               c.Parameters.AddWithValue("@rst", (object?)r.rri_single_time ?? DBNull.Value);
               c.Parameters.AddWithValue("@rt", (object?)r.rri_type ?? DBNull.Value); },
        "SaveAutoAf");

    public void SaveGoal(GoalRequest r) => Exec(@"
        MERGE device_goal AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET step=@s, distance=@d, calorie=@c, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT (device_id,step,distance,calorie) VALUES(@dev,@s,@d,@c);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@s", r.step);
               c.Parameters.AddWithValue("@d", r.distance); c.Parameters.AddWithValue("@c", r.calorie); },
        "SaveGoal");

    public void SaveLanguage(LanguageRequest r) => Exec(@"
        MERGE device_display AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET language=@l, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT (device_id,language) VALUES(@dev,@l);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@l", r.language); },
        "SaveLanguage");

    public void SaveTimeFormat(TimeFormatRequest r) => Exec(@"
        MERGE device_display AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET hour_format=@hf, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT (device_id,hour_format) VALUES(@dev,@hf);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@hf", r.hour_format); },
        "SaveTimeFormat");

    public void SaveDateFormat(DateFormatRequest r) => Exec(@"
        MERGE device_display AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET date_format=@df, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT (device_id,date_format) VALUES(@dev,@df);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@df", r.date_format); },
        "SaveDateFormat");

    public void SaveDistanceUnit(DistanceUnitRequest r) => Exec(@"
        MERGE device_display AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET distance_unit=@du, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT (device_id,distance_unit) VALUES(@dev,@du);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@du", r.distance_unit); },
        "SaveDistanceUnit");

    public void SaveTemperatureUnit(TemperatureUnitRequest r) => Exec(@"
        MERGE device_display AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET temperature_unit=@tu, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT (device_id,temperature_unit) VALUES(@dev,@tu);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@tu", r.temperature_unit); },
        "SaveTemperatureUnit");

    public void SaveWearHand(WearHandRequest r) => Exec(@"
        MERGE device_display AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET wear_hand_right=@wh, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT (device_id,wear_hand_right) VALUES(@dev,@wh);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@wh", r.right); },
        "SaveWearHand");

    public void SaveBpAdjust(BpAdjustRequest r) => Exec(@"
        MERGE device_bp_adjust AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET
            sbp_band=@sb, dbp_band=@db, sbp_meter=@sm, dbp_meter=@dm, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT
            (device_id,sbp_band,dbp_band,sbp_meter,dbp_meter)
            VALUES(@dev,@sb,@db,@sm,@dm);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@sb", r.sbp_band);
               c.Parameters.AddWithValue("@db", r.dbp_band); c.Parameters.AddWithValue("@sm", r.sbp_meter);
               c.Parameters.AddWithValue("@dm", r.dbp_meter); },
        "SaveBpAdjust");

    public void SaveHrInterval(HrIntervalRequest r) => Exec(@"
        MERGE device_hr_interval AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET interval=@iv, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT (device_id,interval) VALUES(@dev,@iv);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@iv", r.interval); },
        "SaveHrInterval");

    public void SaveOtherInterval(OtherIntervalRequest r) => Exec(@"
        MERGE device_other_interval AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET interval=@iv, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT (device_id,interval) VALUES(@dev,@iv);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@iv", r.interval); },
        "SaveOtherInterval");

    public void SaveGpsLocate(GpsLocateRequest r) => Exec(@"
        MERGE device_gps_settings AS t
        USING (SELECT @dev AS device_id) AS s ON t.device_id = s.device_id
        WHEN MATCHED THEN UPDATE SET
            gps_auto_check=@gac, gps_interval_time=@git, run_gps=@rg, updated_at=GETDATE()
        WHEN NOT MATCHED THEN INSERT
            (device_id,gps_auto_check,gps_interval_time,run_gps)
            VALUES(@dev,@gac,@git,@rg);",
        c => { c.Parameters.AddWithValue("@dev", r.device_id); c.Parameters.AddWithValue("@gac", r.gps_auto_check);
               c.Parameters.AddWithValue("@git", r.gps_interval_time); c.Parameters.AddWithValue("@rg", r.run_gps); },
        "SaveGpsLocate");

    // ── multi-row tables: delete + reinsert ────────────────────────────────────

    public void SavePhonebook(PhonebookSyncRequest r)
    {
        try
        {
            using var conn = Open();
            using var del  = new SqlCommand(
                "DELETE FROM device_phonebook WHERE device_id=@dev", conn);
            del.Parameters.AddWithValue("@dev", r.device_id);
            del.ExecuteNonQuery();

            foreach (var contact in r.phone_book.DistinctBy(c => c.number))
            {
                using var ins = new SqlCommand(@"
                    IF NOT EXISTS (SELECT 1 FROM device_phonebook WHERE device_id=@dev AND number=@num)
                    INSERT INTO device_phonebook (device_id,name,number,sos)
                    VALUES (@dev,@n,@num,@s)", conn);
                ins.Parameters.AddWithValue("@dev", r.device_id);
                ins.Parameters.AddWithValue("@n",   contact.name);
                ins.Parameters.AddWithValue("@num", contact.number);
                ins.Parameters.AddWithValue("@s",   contact.sos);
                ins.ExecuteNonQuery();
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "SavePhonebook failed for {Device}", r.device_id); }
    }

    public void ClearPhonebook(string deviceId)
    {
        Exec("DELETE FROM device_phonebook WHERE device_id=@dev",
            c => c.Parameters.AddWithValue("@dev", deviceId), "ClearPhonebook");
    }

    public void SaveClockAlarms(SetAlarmRequest r)
    {
        try
        {
            using var conn = Open();
            using var del  = new SqlCommand(
                "DELETE FROM device_clock_alarms WHERE device_id=@dev", conn);
            del.Parameters.AddWithValue("@dev", r.device_id);
            del.ExecuteNonQuery();

            foreach (var alarm in r.alarms.DistinctBy(a => (a.hour, a.minute)))
            {
                using var ins = new SqlCommand(@"
                    IF NOT EXISTS (SELECT 1 FROM device_clock_alarms WHERE device_id=@dev AND hour=@h AND minute=@m)
                    INSERT INTO device_clock_alarms
                        (device_id,repeat,monday,tuesday,wednesday,thursday,friday,saturday,sunday,hour,minute,title)
                    VALUES (@dev,@rep,@mon,@tue,@wed,@thu,@fri,@sat,@sun,@h,@m,@t)", conn);
                ins.Parameters.AddWithValue("@dev", r.device_id);
                ins.Parameters.AddWithValue("@rep", alarm.repeat);
                ins.Parameters.AddWithValue("@mon", alarm.monday);
                ins.Parameters.AddWithValue("@tue", alarm.tuesday);
                ins.Parameters.AddWithValue("@wed", alarm.wednesday);
                ins.Parameters.AddWithValue("@thu", alarm.thursday);
                ins.Parameters.AddWithValue("@fri", alarm.friday);
                ins.Parameters.AddWithValue("@sat", alarm.saturday);
                ins.Parameters.AddWithValue("@sun", alarm.sunday);
                ins.Parameters.AddWithValue("@h",   alarm.hour);
                ins.Parameters.AddWithValue("@m",   alarm.minute);
                ins.Parameters.AddWithValue("@t",   alarm.title);
                ins.ExecuteNonQuery();
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "SaveClockAlarms failed for {Device}", r.device_id); }
    }

    public void ClearClockAlarms(string deviceId)
    {
        Exec("DELETE FROM device_clock_alarms WHERE device_id=@dev",
            c => c.Parameters.AddWithValue("@dev", deviceId), "ClearClockAlarms");
    }

    public void SaveSedentary(SetSedentaryRequest r)
    {
        try
        {
            using var conn = Open();
            using var del  = new SqlCommand(
                "DELETE FROM device_sedentary WHERE device_id=@dev", conn);
            del.Parameters.AddWithValue("@dev", r.device_id);
            del.ExecuteNonQuery();

            foreach (var s in r.sedentaries.DistinctBy(x => (x.start_hour, x.end_hour)))
            {
                using var ins = new SqlCommand(@"
                    IF NOT EXISTS (SELECT 1 FROM device_sedentary WHERE device_id=@dev AND start_hour=@sh AND end_hour=@eh)
                    INSERT INTO device_sedentary
                        (device_id,repeat,monday,tuesday,wednesday,thursday,friday,saturday,sunday,
                         start_hour,end_hour,duration,threshold)
                    VALUES (@dev,@rep,@mon,@tue,@wed,@thu,@fri,@sat,@sun,@sh,@eh,@dur,@thr)", conn);
                ins.Parameters.AddWithValue("@dev", r.device_id);
                ins.Parameters.AddWithValue("@rep", s.repeat);
                ins.Parameters.AddWithValue("@mon", s.monday);
                ins.Parameters.AddWithValue("@tue", s.tuesday);
                ins.Parameters.AddWithValue("@wed", s.wednesday);
                ins.Parameters.AddWithValue("@thu", s.thursday);
                ins.Parameters.AddWithValue("@fri", s.friday);
                ins.Parameters.AddWithValue("@sat", s.saturday);
                ins.Parameters.AddWithValue("@sun", s.sunday);
                ins.Parameters.AddWithValue("@sh",  s.start_hour);
                ins.Parameters.AddWithValue("@eh",  s.end_hour);
                ins.Parameters.AddWithValue("@dur", s.duration);
                ins.Parameters.AddWithValue("@thr", s.threshold);
                ins.ExecuteNonQuery();
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "SaveSedentary failed for {Device}", r.device_id); }
    }

    public void ClearSedentary(string deviceId)
    {
        Exec("DELETE FROM device_sedentary WHERE device_id=@dev",
            c => c.Parameters.AddWithValue("@dev", deviceId), "ClearSedentary");
    }
}
