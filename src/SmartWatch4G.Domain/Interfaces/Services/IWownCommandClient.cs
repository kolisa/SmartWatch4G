namespace SmartWatch4G.Domain.Interfaces.Services;

/// <summary>
/// Client for the iwown entservice — used to send commands to devices and
/// query device status.
///
/// China Mainland : https://search.iwown.com
/// Rest of world  : https://euapi.iwown.com
///
/// Authentication: HTTP headers  account = your_account,
///                                pwd     = MD5(your_password)
///
/// Composed from focused sub-interfaces: <see cref="IDeviceProfileClient"/>,
/// <see cref="IDeviceLocationClient"/>, <see cref="IDeviceAlarmConfigClient"/>,
/// <see cref="IDeviceMeasurementClient"/>, <see cref="IDevicePhonebookClient"/>,
/// <see cref="IDeviceStatusClient"/>.
/// </summary>
public interface IWownCommandClient :
    IDeviceProfileClient,
    IDeviceLocationClient,
    IDeviceAlarmConfigClient,
    IDeviceMeasurementClient,
    IDevicePhonebookClient,
    IDeviceStatusClient
{
}

// ─── Return types ─────────────────────────────────────────────────────────────

/// <summary>Generic command result.</summary>
public sealed record CommandResult(int ReturnCode, bool Success)
{
    public static CommandResult Ok => new(0, true);
}

/// <summary>Device online status returned by GET /entservice/device/status.</summary>
public sealed record DeviceOnlineStatus(string DeviceId, int StatusCode)
{
    /// <summary>0=Offline, 1=Online, 2=Unactivated, 3=Disabled, 4=Unactivated/Not Exist.</summary>
    public bool IsOnline => StatusCode == 1;
}

// ─── Command models ────────────────────────────────────────────────────────────

public sealed record UserInfoCommand(
    string DeviceId,
    int Height,
    int Weight,
    int Gender,
    int Age,
    int WristCircle = 0,
    int Hypertension = 0);

public sealed record PhonebookEntry(string Name, string Number, bool IsSos);

public sealed record PhonebookCommand(
    string DeviceId,
    IReadOnlyList<PhonebookEntry> PhoneBook,
    int? Forbid = null);

public sealed record DataFrequencyCommand(
    string DeviceId,
    bool GpsAutoCheck,
    int GpsIntervalMinutes,
    int? PowerMode = null);

public sealed record LocateDataUploadCommand(
    string DeviceId,
    bool DataAutoUpload,
    int DataUploadIntervalMinutes,
    bool AutoLocate,
    int LocateIntervalMinutes,
    int? PowerMode = null);

public sealed record WristGestureCommand(
    string DeviceId,
    bool Open,
    int StartHour,
    int EndHour);

public sealed record HrAlarmCommand(
    string DeviceId,
    bool Open,
    int High,
    int Low,
    int Threshold,
    int AlarmIntervalMinutes);

public sealed record DynamicHrAlarmCommand(
    string DeviceId,
    bool Open,
    int High,
    int Low,
    int TimeoutSeconds,
    int IntervalMinutes);

public sealed record BpAlarmCommand(
    string DeviceId,
    bool Open,
    int SbpHigh,
    int SbpBelow,
    int DbpHigh,
    int DbpBelow);

public sealed record AutoAfCommand(
    string DeviceId,
    bool Open,
    int IntervalSeconds,
    bool? RriSingleTime = null,
    int? RriType = null);

public sealed record ClockAlarmEntry(
    bool Repeat,
    bool Monday, bool Tuesday, bool Wednesday, bool Thursday,
    bool Friday, bool Saturday, bool Sunday,
    int Hour, int Minute,
    string Title);

public sealed record ClockAlarmCommand(
    string DeviceId,
    IReadOnlyList<ClockAlarmEntry> Alarms);

public sealed record SedentaryEntry(
    bool Repeat,
    bool Monday, bool Tuesday, bool Wednesday, bool Thursday,
    bool Friday, bool Saturday, bool Sunday,
    int StartHour, int EndHour,
    int Duration, int Threshold);

public sealed record SedentaryCommand(
    string DeviceId,
    IReadOnlyList<SedentaryEntry> Sedentaries);

public sealed record GpsLocateCommand(
    string DeviceId,
    bool GpsAutoCheck,
    int GpsIntervalMinutes,
    bool RunGps);

public sealed record BpCalibrationCommand(
    string DeviceId,
    int SbpBand, int DbpBand,
    int SbpMeter, int DbpMeter);
