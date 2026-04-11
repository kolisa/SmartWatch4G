using System.ComponentModel.DataAnnotations;

namespace SmartWatch4G.Application.DTOs;

// ── Generic command response ──────────────────────────────────────────────────

/// <summary>Wraps the result of a device command sent via the iwown entservice.</summary>
public sealed class CommandResultDto
{
    public int ReturnCode { get; set; }
    public bool Success { get; set; }
}

/// <summary>Wraps the device online/offline status.</summary>
public sealed class DeviceOnlineStatusDto
{
    public int ReturnCode { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    /// <summary>0=Offline, 1=Online, 2=Unactivated, 3=Disabled, 4=Unactivated/NotExist</summary>
    public int StatusCode { get; set; }
    public bool IsOnline { get; set; }
}

// ── Core device commands ──────────────────────────────────────────────────────

/// <summary>Push user profile to a device.</summary>
public sealed class SendUserInfoRequest
{
    /// <summary>Height in cm (50–250).</summary>
    [Range(50, 250)]
    public int Height { get; set; }
    /// <summary>Weight in kg (20–300).</summary>
    [Range(20, 300)]
    public int Weight { get; set; }
    /// <summary>1 = male, 2 = female.</summary>
    [Range(1, 2)]
    public int Gender { get; set; }
    [Range(0, 120)]
    public int Age { get; set; }
    /// <summary>Wrist circumference in mm (80–230).</summary>
    [Range(80, 230)]
    public int WristCircle { get; set; }
    /// <summary>1 = hypertensive, 2 = not hypertensive.</summary>
    [Range(1, 2)]
    public int Hypertension { get; set; }
}

/// <summary>Push a notification message to a device.</summary>
public sealed class SendMessageRequest
{
    /// <summary>Title — max 15 bytes.</summary>
    [Required]
    [StringLength(15)]
    public string Title { get; set; } = string.Empty;
    /// <summary>Body — max 240 bytes.</summary>
    [Required]
    [StringLength(240)]
    public string Description { get; set; } = string.Empty;
}

/// <summary>Set the display language on a device.</summary>
public sealed class SetLanguageRequest
{
    /// <summary>
    /// 0=English, 1=Chinese(Simplified), 2=Italian, 3=Japanese, 4=French,
    /// 5=German, 6=Portuguese, 7=Spanish, 8=Russian, 9=Korean, 10=Arabic,
    /// 11=Vietnamese, 12=Polish, 13=Romanian, 14=Swedish, 15=Thai, 16=Turkish,
    /// 17=Danish, 18=Ukrainian, 19=Norwegian, 20=Dutch, 21=Czech,
    /// 22=Chinese(Traditional), 23=Indonesian.
    /// </summary>
    [Range(0, 23)]
    public int LanguageCode { get; set; }
}

/// <summary>Set daily activity goals.</summary>
public sealed class SetGoalRequest
{
    [Range(0, 100_000)]
    public int Step { get; set; }
    [Range(0, 100_000)]
    public int DistanceMetres { get; set; }
    [Range(0, 10_000)]
    public int CalorieKcal { get; set; }
}

// ── Health alert commands ─────────────────────────────────────────────────────

/// <summary>Configure static heart-rate alarm.</summary>
public sealed class SetHrAlarmRequest
{
    public bool Open { get; set; }
    [Range(40, 220)]
    public int High { get; set; }
    [Range(40, 220)]
    public int Low { get; set; }
    /// <summary>Number of consecutive abnormal readings before alert fires.</summary>
    [Range(1, 10)]
    public int Threshold { get; set; }
    [Range(1, 1440)]
    public int AlarmIntervalMinutes { get; set; }
}

/// <summary>Configure dynamic heart-rate alarm.</summary>
public sealed class SetDynamicHrAlarmRequest
{
    public bool Open { get; set; }
    [Range(40, 220)]
    public int High { get; set; }
    [Range(40, 220)]
    public int Low { get; set; }
    /// <summary>Duration of sustained abnormal HR before alert (seconds).</summary>
    [Range(1, 3600)]
    public int TimeoutSeconds { get; set; }
    [Range(1, 1440)]
    public int IntervalMinutes { get; set; }
}

/// <summary>Configure blood-oxygen (SpO2) alarm.</summary>
public sealed class SetSpo2AlarmRequest
{
    public bool Open { get; set; }
    [Range(70, 99)]
    public int LowThreshold { get; set; }
}

/// <summary>Configure blood-pressure alarm thresholds.</summary>
public sealed class SetBpAlarmRequest
{
    public bool Open { get; set; }
    [Range(60, 280)]
    public int SbpHigh { get; set; }
    [Range(60, 280)]
    public int SbpBelow { get; set; }
    [Range(40, 180)]
    public int DbpHigh { get; set; }
    [Range(40, 180)]
    public int DbpBelow { get; set; }
}

/// <summary>Configure body-temperature alarm thresholds.</summary>
public sealed class SetTemperatureAlarmRequest
{
    public bool Open { get; set; }
    /// <summary>High threshold — value × 10 (e.g. 375 = 37.5 °C, range 340–420).</summary>
    [Range(340, 420)]
    public int High { get; set; }
    /// <summary>Low threshold — value × 10 (e.g. 355 = 35.5 °C, range 340–420).</summary>
    [Range(340, 420)]
    public int Low { get; set; }
}

/// <summary>Configure blood-sugar alarm.</summary>
public sealed class SetBloodSugarAlarmRequest
{
    public bool Open { get; set; }
    [Range(0.0, 30.0)]
    public double Low { get; set; }
    [Range(0.0, 30.0)]
    public double High { get; set; }
}

/// <summary>Configure blood-potassium alarm.</summary>
public sealed class SetBloodPotassiumAlarmRequest
{
    public bool Open { get; set; }
    [Range(0.0, 10.0)]
    public double Low { get; set; }
    [Range(0.0, 10.0)]
    public double High { get; set; }
}

/// <summary>Configure auto atrial-fibrillation (AF) monitoring.</summary>
public sealed class SetAutoAfRequest
{
    public bool Open { get; set; }
    /// <summary>Measurement interval in seconds (30–120).</summary>
    [Range(30, 120)]
    public int IntervalSeconds { get; set; }
    public bool? RriSingleTime { get; set; }
    /// <summary>0 = general, 1 = mood.</summary>
    [Range(0, 1)]
    public int? RriType { get; set; }
}

// ── Device settings commands ──────────────────────────────────────────────────

/// <summary>Set fall-detection sensitivity.</summary>
public sealed class SetFallSensitivityRequest
{
    /// <summary>Default: 14000.</summary>
    [Range(1000, 30000)]
    public int FallThreshold { get; set; } = 14000;
}

/// <summary>Configure wrist-raise wake-screen gesture window.</summary>
public sealed class SetWristGestureRequest
{
    public bool Open { get; set; }
    [Range(0, 23)]
    public int StartHour { get; set; }
    [Range(0, 23)]
    public int EndHour { get; set; }
}

/// <summary>Set data-upload and GPS auto-locate intervals (combined power-mode).</summary>
public sealed class SetDataFrequencyRequest
{
    public bool GpsAutoCheck { get; set; }
    [Range(1, 1440)]
    public int GpsIntervalMinutes { get; set; }
    /// <summary>1 = low (max saving), 2 = mid (balanced), 3 = high (always connected).</summary>
    [Range(1, 3)]
    public int? PowerMode { get; set; }
}

/// <summary>Set data-upload and auto-locate intervals independently.</summary>
public sealed class SetLocateDataUploadRequest
{
    public bool DataAutoUpload { get; set; }
    [Range(1, 1440)]
    public int DataUploadIntervalMinutes { get; set; }
    public bool AutoLocate { get; set; }
    [Range(1, 1440)]
    public int LocateIntervalMinutes { get; set; }
    /// <summary>1 = low, 2 = mid, 3 = high.</summary>
    [Range(1, 3)]
    public int? PowerMode { get; set; }
}

/// <summary>Set heart-rate measurement interval.</summary>
public sealed class SetHrMeasureIntervalRequest
{
    /// <summary>Minimum 1 minute.</summary>
    [Range(1, 1440)]
    public int IntervalMinutes { get; set; }
}

/// <summary>Set non-HR health measurement interval (SpO2, BP, stress).</summary>
public sealed class SetOtherMeasureIntervalRequest
{
    /// <summary>Minimum 5 minutes.</summary>
    [Range(5, 1440)]
    public int IntervalMinutes { get; set; }
}

/// <summary>Configure GPS locate settings.</summary>
public sealed class SetGpsLocateRequest
{
    public bool GpsAutoCheck { get; set; }
    [Range(1, 1440)]
    public int GpsIntervalMinutes { get; set; }
    /// <summary>Force GPS to run immediately.</summary>
    public bool RunGps { get; set; }
}

/// <summary>Set time display format.</summary>
public sealed class SetTimeFormatRequest
{
    /// <summary>true = 12-hour, false = 24-hour.</summary>
    public bool Use12Hour { get; set; }
}

/// <summary>Set date display format.</summary>
public sealed class SetDateFormatRequest
{
    /// <summary>true = DD/MM, false = MM/DD.</summary>
    public bool DayFirst { get; set; }
}

/// <summary>Set distance unit.</summary>
public sealed class SetDistanceUnitRequest
{
    /// <summary>true = imperial (miles), false = metric (km).</summary>
    public bool Imperial { get; set; }
}

/// <summary>Set temperature unit.</summary>
public sealed class SetTemperatureUnitRequest
{
    /// <summary>true = Fahrenheit, false = Celsius.</summary>
    public bool Fahrenheit { get; set; }
}

/// <summary>Set which wrist the device is worn on.</summary>
public sealed class SetWearHandRequest
{
    /// <summary>true = right hand, false = left hand.</summary>
    public bool RightHand { get; set; }
}

/// <summary>Set blood-pressure calibration reference values.</summary>
public sealed class SetBpCalibrationRequest
{
    [Range(60, 280)]
    public int SbpBand { get; set; }
    [Range(40, 180)]
    public int DbpBand { get; set; }
    [Range(60, 280)]
    public int SbpMeter { get; set; }
    [Range(40, 180)]
    public int DbpMeter { get; set; }
}

/// <summary>Set a blood-pressure measurement schedule.</summary>
public sealed class SetBpScheduleRequest
{
    /// <summary>Up to 48 time strings (e.g. "08:00", "14:30").</summary>
    [MaxLength(48)]
    public IReadOnlyList<string> MeasureTimes { get; set; } = [];
}

// ── Phonebook commands ────────────────────────────────────────────────────────

/// <summary>Sync phonebook to a device (max 8 entries, at least 1 SOS).</summary>
public sealed class SyncPhonebookRequest
{
    [MaxLength(8)]
    public IReadOnlyList<PhonebookEntryRequest> PhoneBook { get; set; } = [];
    /// <summary>1 = block all non-phonebook calls, 2 = allow all calls.</summary>
    [Range(1, 2)]
    public int? Forbid { get; set; }
}

public sealed class PhonebookEntryRequest
{
    /// <summary>Max 24 bytes.</summary>
    [Required]
    [StringLength(24)]
    public string Name { get; set; } = string.Empty;
    /// <summary>Max 20 bytes.</summary>
    [Required]
    [StringLength(20)]
    public string Number { get; set; } = string.Empty;
    public bool IsSos { get; set; }
}

// ── Schedule commands ─────────────────────────────────────────────────────────

/// <summary>Set clock alarms on a device (max 5).</summary>
public sealed class SetClockAlarmsRequest
{
    [MaxLength(5)]
    public IReadOnlyList<ClockAlarmEntryRequest> Alarms { get; set; } = [];
}

public sealed class ClockAlarmEntryRequest
{
    public bool Repeat { get; set; }
    public bool Monday { get; set; }
    public bool Tuesday { get; set; }
    public bool Wednesday { get; set; }
    public bool Thursday { get; set; }
    public bool Friday { get; set; }
    public bool Saturday { get; set; }
    public bool Sunday { get; set; }
    [Range(0, 23)]
    public int Hour { get; set; }
    [Range(0, 59)]
    public int Minute { get; set; }
    [StringLength(20)]
    public string Title { get; set; } = string.Empty;
}

/// <summary>Set sedentary reminders on a device (max 3).</summary>
public sealed class SetSedentaryRequest
{
    [MaxLength(3)]
    public IReadOnlyList<SedentaryEntryRequest> Sedentaries { get; set; } = [];
}

public sealed class SedentaryEntryRequest
{
    public bool Repeat { get; set; }
    public bool Monday { get; set; }
    public bool Tuesday { get; set; }
    public bool Wednesday { get; set; }
    public bool Thursday { get; set; }
    public bool Friday { get; set; }
    public bool Saturday { get; set; }
    public bool Sunday { get; set; }
    [Range(0, 23)]
    public int StartHour { get; set; }
    [Range(0, 23)]
    public int EndHour { get; set; }
    /// <summary>Sedentary duration threshold (units × 5 minutes).</summary>
    [Range(1, 288)]
    public int Duration { get; set; }
    /// <summary>Step count per minute threshold.</summary>
    [Range(0, 1000)]
    public int Threshold { get; set; }
}
