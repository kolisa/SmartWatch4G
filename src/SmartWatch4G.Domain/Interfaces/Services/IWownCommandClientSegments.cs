namespace SmartWatch4G.Domain.Interfaces.Services;

/// <summary>
/// User profile and general device management commands.
/// </summary>
public interface IDeviceProfileClient
{
    /// <summary>Push user profile (height, weight, age, gender) to a device.</summary>
    Task<CommandResult> SendUserInfoAsync(UserInfoCommand cmd, CancellationToken ct = default);

    /// <summary>Set daily step/distance/calorie goals.</summary>
    Task<CommandResult> SetGoalAsync(string deviceId, int step, int distanceMetres, int calorieKcal, CancellationToken ct = default);

    /// <summary>Set the display language on a device.</summary>
    Task<CommandResult> SetLanguageAsync(string deviceId, int languageCode, CancellationToken ct = default);

    /// <summary>Set time display format (12 or 24 hour).</summary>
    Task<CommandResult> SetTimeFormatAsync(string deviceId, bool use12Hour, CancellationToken ct = default);

    /// <summary>Set date display format (MM/DD or DD/MM).</summary>
    Task<CommandResult> SetDateFormatAsync(string deviceId, bool dayFirst, CancellationToken ct = default);

    /// <summary>Set distance unit (metric or imperial).</summary>
    Task<CommandResult> SetDistanceUnitAsync(string deviceId, bool imperial, CancellationToken ct = default);

    /// <summary>Set temperature unit (Celsius or Fahrenheit).</summary>
    Task<CommandResult> SetTemperatureUnitAsync(string deviceId, bool fahrenheit, CancellationToken ct = default);

    /// <summary>Set which wrist the device is worn on.</summary>
    Task<CommandResult> SetWearHandAsync(string deviceId, bool rightHand, CancellationToken ct = default);

    /// <summary>Factory-reset a device.</summary>
    Task<CommandResult> FactoryResetAsync(string deviceId, CancellationToken ct = default);

    /// <summary>Trigger a one-shot data sync from a device.</summary>
    Task<CommandResult> TriggerDataSyncAsync(string deviceId, CancellationToken ct = default);

    /// <summary>Push a notification message to a device.</summary>
    Task<CommandResult> SendMessageAsync(string deviceId, string title, string description, CancellationToken ct = default);
}

/// <summary>
/// Location, GPS, and realtime-position commands.
/// </summary>
public interface IDeviceLocationClient
{
    /// <summary>Request an immediate real-time GPS location fix from a device.</summary>
    Task<CommandResult> RequestRealtimeLocationAsync(string deviceId, CancellationToken ct = default);

    /// <summary>Set data upload / GPS auto-locate intervals (combined).</summary>
    Task<CommandResult> SetDataFrequencyAsync(DataFrequencyCommand cmd, CancellationToken ct = default);

    /// <summary>Set data upload and auto-locate intervals independently.</summary>
    Task<CommandResult> SetLocateDataUploadFrequencyAsync(LocateDataUploadCommand cmd, CancellationToken ct = default);

    /// <summary>Configure GPS locate settings.</summary>
    Task<CommandResult> SetGpsLocateAsync(GpsLocateCommand cmd, CancellationToken ct = default);
}

/// <summary>
/// Health measurement alarm configuration commands.
/// </summary>
public interface IDeviceAlarmConfigClient
{
    /// <summary>Configure static heart-rate alarm thresholds.</summary>
    Task<CommandResult> SetHrAlarmAsync(HrAlarmCommand cmd, CancellationToken ct = default);

    /// <summary>Configure dynamic heart-rate alarm thresholds.</summary>
    Task<CommandResult> SetDynamicHrAlarmAsync(DynamicHrAlarmCommand cmd, CancellationToken ct = default);

    /// <summary>Configure blood-oxygen alarm threshold.</summary>
    Task<CommandResult> SetSpo2AlarmAsync(string deviceId, bool open, int lowThreshold, CancellationToken ct = default);

    /// <summary>Configure blood-pressure alarm thresholds.</summary>
    Task<CommandResult> SetBpAlarmAsync(BpAlarmCommand cmd, CancellationToken ct = default);

    /// <summary>Configure body-temperature alarm thresholds (value × 10, e.g. 37.5° = 375).</summary>
    Task<CommandResult> SetTemperatureAlarmAsync(string deviceId, bool open, int high, int low, CancellationToken ct = default);

    /// <summary>Configure blood-sugar alarm thresholds.</summary>
    Task<CommandResult> SetBloodSugarAlarmAsync(string deviceId, bool open, double low, double high, CancellationToken ct = default);

    /// <summary>Configure blood-potassium alarm thresholds.</summary>
    Task<CommandResult> SetBloodPotassiumAlarmAsync(string deviceId, bool open, double low, double high, CancellationToken ct = default);

    /// <summary>Set alarms on a device (up to 5).</summary>
    Task<CommandResult> SetAlarmsAsync(ClockAlarmCommand cmd, CancellationToken ct = default);

    /// <summary>Clear all alarms on a device.</summary>
    Task<CommandResult> ClearAlarmsAsync(string deviceId, CancellationToken ct = default);

    /// <summary>Set sedentary reminders (up to 3).</summary>
    Task<CommandResult> SetSedentaryAsync(SedentaryCommand cmd, CancellationToken ct = default);

    /// <summary>Clear all sedentary reminders on a device.</summary>
    Task<CommandResult> ClearSedentaryAsync(string deviceId, CancellationToken ct = default);

    /// <summary>Enable or disable fall detection on a device.</summary>
    Task<CommandResult> SetFallDetectionAsync(string deviceId, bool enabled, CancellationToken ct = default);

    /// <summary>Set fall-detection sensitivity (default 14000).</summary>
    Task<CommandResult> SetFallSensitivityAsync(string deviceId, int fallThreshold, CancellationToken ct = default);
}

/// <summary>
/// Health measurement interval and scheduling commands.
/// </summary>
public interface IDeviceMeasurementClient
{
    /// <summary>Configure auto-AF (atrial fibrillation) monitoring.</summary>
    Task<CommandResult> SetAutoAfAsync(AutoAfCommand cmd, CancellationToken ct = default);

    /// <summary>Set heart-rate measurement interval (minutes, minimum 1).</summary>
    Task<CommandResult> SetHrMeasureIntervalAsync(string deviceId, int intervalMinutes, CancellationToken ct = default);

    /// <summary>Set non-HR health measurement interval (SpO2, BP, stress — minimum 5 minutes).</summary>
    Task<CommandResult> SetOtherMeasureIntervalAsync(string deviceId, int intervalMinutes, CancellationToken ct = default);

    /// <summary>Set a blood-pressure measurement schedule (up to 48 time-points).</summary>
    Task<CommandResult> SetBpMeasureScheduleAsync(string deviceId, IReadOnlyList<string> measureTimes, CancellationToken ct = default);

    /// <summary>Set blood-pressure calibration reference values.</summary>
    Task<CommandResult> SetBpCalibrationAsync(BpCalibrationCommand cmd, CancellationToken ct = default);

    /// <summary>Set wrist-raise wake-screen window.</summary>
    Task<CommandResult> SetWristGestureAsync(WristGestureCommand cmd, CancellationToken ct = default);
}

/// <summary>
/// Phonebook management commands.
/// </summary>
public interface IDevicePhonebookClient
{
    /// <summary>Push a full phonebook (up to 8 entries, at least 1 SOS) to a device.</summary>
    Task<CommandResult> SyncPhonebookAsync(PhonebookCommand cmd, CancellationToken ct = default);

    /// <summary>Clear the phonebook on a device.</summary>
    Task<CommandResult> ClearPhonebookAsync(string deviceId, CancellationToken ct = default);
}

/// <summary>
/// Device status query commands.
/// </summary>
public interface IDeviceStatusClient
{
    /// <summary>Query the current online/offline status of a device.</summary>
    Task<DeviceOnlineStatus?> GetDeviceStatusAsync(string deviceId, CancellationToken ct = default);
}
