namespace SmartWatch4G.Application.DTOs;

public class IwownResponse
{
    public int ReturnCode { get; set; }
    public object? Data { get; set; }
}

public class DeviceIdRequest
{
    public string device_id { get; set; } = string.Empty;
}

public class UserInfoRequest
{
    public string device_id { get; set; } = string.Empty;
    public int height { get; set; }
    public int weight { get; set; }
    public int gender { get; set; }
    public int age { get; set; }
    public int calibrate_walk { get; set; }
    public int calibrate_run { get; set; }
    public int wrist_circle { get; set; }
    public int? hypertension { get; set; }
}

public class FallCheckRequest
{
    public string device_id { get; set; } = string.Empty;
    public bool fall_check { get; set; }
}

public class PhoneContact
{
    public string name { get; set; } = string.Empty;
    public string number { get; set; } = string.Empty;
    public bool sos { get; set; }
}

public class PhonebookSyncRequest
{
    public string device_id { get; set; } = string.Empty;
    public List<PhoneContact> phone_book { get; set; } = new();
    public int? forbid { get; set; }
}

public class DataFreqRequest
{
    public string device_id { get; set; } = string.Empty;
    public bool gps_auto_check { get; set; }
    public int gps_interval_time { get; set; }
    public int? power_mode { get; set; }
}

public class LocateDataUploadFreqRequest
{
    public string device_id { get; set; } = string.Empty;
    public bool data_auto_upload { get; set; }
    public int data_upload_interval { get; set; }
    public bool auto_locate { get; set; }
    public int locate_interval_time { get; set; }
    public int? power_mode { get; set; }
}

public class LcdGestureRequest
{
    public string device_id { get; set; } = string.Empty;
    public bool open { get; set; }
    public int start_hour { get; set; }
    public int end_hour { get; set; }
}

public class HrAlarmRequest
{
    public string device_id { get; set; } = string.Empty;
    public bool open { get; set; }
    public int high { get; set; }
    public int low { get; set; }
    public int threshold { get; set; }
    public int alarm_interval { get; set; }
}

public class DynamicHrAlarmRequest
{
    public string device_id { get; set; } = string.Empty;
    public bool open { get; set; }
    public int high { get; set; }
    public int low { get; set; }
    public int timeout { get; set; }
    public int interval { get; set; }
}

public class Spo2AlarmRequest
{
    public string device_id { get; set; } = string.Empty;
    public bool open { get; set; }
    public int low { get; set; }
}

public class BpAlarmRequest
{
    public string device_id { get; set; } = string.Empty;
    public bool open { get; set; }
    public int sbp_high { get; set; }
    public int sbp_below { get; set; }
    public int dbp_high { get; set; }
    public int dbp_below { get; set; }
}

public class TemperatureAlarmRequest
{
    public string device_id { get; set; } = string.Empty;
    public bool open { get; set; }
    public int high { get; set; }
    public int low { get; set; }
}

public class AutoAfRequest
{
    public string device_id { get; set; } = string.Empty;
    public bool open { get; set; }
    public int interval { get; set; } = 60;
    public bool? rri_single_time { get; set; }
    public int? rri_type { get; set; }
}

public class ClockAlarm
{
    public bool repeat { get; set; }
    public bool monday { get; set; }
    public bool tuesday { get; set; }
    public bool wednesday { get; set; }
    public bool thursday { get; set; }
    public bool friday { get; set; }
    public bool saturday { get; set; }
    public bool sunday { get; set; }
    public int hour { get; set; }
    public int minute { get; set; }
    public string title { get; set; } = string.Empty;
}

public class SetAlarmRequest
{
    public string device_id { get; set; } = string.Empty;
    public List<ClockAlarm> alarms { get; set; } = new();
}

public class SedentaryReminder
{
    public bool repeat { get; set; }
    public bool monday { get; set; }
    public bool tuesday { get; set; }
    public bool wednesday { get; set; }
    public bool thursday { get; set; }
    public bool friday { get; set; }
    public bool saturday { get; set; }
    public bool sunday { get; set; }
    public int start_hour { get; set; }
    public int end_hour { get; set; }
    public int duration { get; set; }
    public int threshold { get; set; } = 40;
}

public class SetSedentaryRequest
{
    public string device_id { get; set; } = string.Empty;
    public List<SedentaryReminder> sedentaries { get; set; } = new();
}

public class GoalRequest
{
    public string device_id { get; set; } = string.Empty;
    public int step { get; set; }
    public int distance { get; set; }
    public int calorie { get; set; }
}

public class LanguageRequest
{
    public string device_id { get; set; } = string.Empty;
    public int language { get; set; }
}

public class MessageRequest
{
    public string device_id { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
}

public class FallCheckSensitivityRequest
{
    public string device_id { get; set; } = string.Empty;
    public int fall_threshold { get; set; } = 14000;
}

public class HrIntervalRequest
{
    public string device_id { get; set; } = string.Empty;
    public int interval { get; set; }
}

public class OtherIntervalRequest
{
    public string device_id { get; set; } = string.Empty;
    public int interval { get; set; }
}

public class GpsLocateRequest
{
    public string device_id { get; set; } = string.Empty;
    public bool gps_auto_check { get; set; }
    public int gps_interval_time { get; set; }
    public bool run_gps { get; set; }
}

public class TimeFormatRequest
{
    public string device_id { get; set; } = string.Empty;
    public int hour_format { get; set; }
}

public class DateFormatRequest
{
    public string device_id { get; set; } = string.Empty;
    public int date_format { get; set; }
}

public class DistanceUnitRequest
{
    public string device_id { get; set; } = string.Empty;
    public int distance_unit { get; set; }
}

public class TemperatureUnitRequest
{
    public string device_id { get; set; } = string.Empty;
    public int temperature_unit { get; set; }
}

public class WearHandRequest
{
    public string device_id { get; set; } = string.Empty;
    public bool right { get; set; }
}

public class BpAdjustRequest
{
    public string device_id { get; set; } = string.Empty;
    public int sbp_band { get; set; }
    public int dbp_band { get; set; }
    public int sbp_meter { get; set; }
    public int dbp_meter { get; set; }
}

public class SleepCalculationRequest
{
    public string prevDay { get; set; } = string.Empty;
    public string nextDay { get; set; } = string.Empty;
    public int[]? prevDayRri { get; set; }
    public int[]? nextDayRri { get; set; }
    public string recordDate { get; set; } = string.Empty;
    public string device_id { get; set; } = string.Empty;
    public string account { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
}

public class SleepSection
{
    public string? start_time { get; set; }
    public string? end_time { get; set; }
    public int type { get; set; }
}

public class SleepRespiratoryStats
{
    public double avg { get; set; }
    public double max { get; set; }
    public double min { get; set; }
}

public class SleepCalculationResponse
{
    public int ReturnCode { get; set; }
    public string? message { get; set; }
    public int completed { get; set; }
    public string? start_time { get; set; }
    public string? end_time { get; set; }
    public int hr { get; set; }
    public int turn_times { get; set; }
    public SleepRespiratoryStats? respiratory { get; set; }
    public List<SleepSection>? sections { get; set; }
}

public class EcgCalculationRequest
{
    public int[]? ecg_list { get; set; }
    public string device_id { get; set; } = string.Empty;
    public string account { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
}

public class EcgCalculationResponse
{
    public int ReturnCode { get; set; }
    public string? message { get; set; }
    public int result { get; set; }
    public int hr { get; set; }
    public int effective { get; set; }
    public int direction { get; set; }
}

public class AfCalculationRequest
{
    public int[]? rri_list { get; set; }
    public string device_id { get; set; } = string.Empty;
    public string account { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
}

public class AfCalculationResponse
{
    public int ReturnCode { get; set; }
    public string? message { get; set; }
    public int result { get; set; }
}

public class Spo2CalculationRequest
{
    public int[]? spo2_list { get; set; }
    public string device_id { get; set; } = string.Empty;
    public string account { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
}

public class Spo2CalculationResponse
{
    public int ReturnCode { get; set; }
    public string? message { get; set; }
    public double spo2_score { get; set; }
    public int? osahs_risk { get; set; }
}
