namespace SmartWatch4G.Application.DTOs;

// ── Response DTOs for device command configurations ───────────────────────────

public sealed class DeviceConfigResponse
{
    public string  DeviceId  { get; init; } = string.Empty;
    public string? UserName  { get; init; }

    // Data / locate frequencies
    public bool?   GpsAutoCheck        { get; init; }
    public int?    GpsIntervalTime     { get; init; }
    public int?    PowerMode           { get; init; }
    public bool?   DataAutoUpload      { get; init; }
    public int?    DataUploadInterval  { get; init; }
    public bool?   AutoLocate          { get; init; }
    public int?    LocateIntervalTime  { get; init; }

    // HR alarm
    public bool? HrAlarmOpen          { get; init; }
    public int?  HrAlarmHigh          { get; init; }
    public int?  HrAlarmLow           { get; init; }
    public int?  HrAlarmThreshold     { get; init; }
    public int?  HrAlarmInterval      { get; init; }

    // Dynamic HR alarm
    public bool? DynHrAlarmOpen       { get; init; }
    public int?  DynHrAlarmHigh       { get; init; }
    public int?  DynHrAlarmLow        { get; init; }
    public int?  DynHrAlarmTimeout    { get; init; }
    public int?  DynHrAlarmInterval   { get; init; }

    // SpO2 alarm
    public bool? Spo2AlarmOpen        { get; init; }
    public int?  Spo2AlarmLow         { get; init; }

    // BP alarm
    public bool? BpAlarmOpen          { get; init; }
    public int?  BpSbpHigh            { get; init; }
    public int?  BpSbpBelow           { get; init; }
    public int?  BpDbpHigh            { get; init; }
    public int?  BpDbpBelow           { get; init; }

    // Temperature alarm
    public bool?   TempAlarmOpen      { get; init; }
    public double? TempAlarmHigh      { get; init; }
    public double? TempAlarmLow       { get; init; }

    // Fall check
    public bool? FallCheckEnabled     { get; init; }
    public int?  FallThreshold        { get; init; }

    // Display / units
    public string? Language           { get; init; }
    public int?    HourFormat         { get; init; }
    public string? DateFormat         { get; init; }
    public int?    DistanceUnit       { get; init; }
    public int?    TemperatureUnit    { get; init; }
    public bool?   WearHandRight      { get; init; }

    // Measurement intervals
    public int?  HrInterval           { get; init; }
    public int?  OtherInterval        { get; init; }

    // Goal
    public int?    GoalStep           { get; init; }
    public double? GoalDistance       { get; init; }
    public double? GoalCalorie        { get; init; }

    // GPS locate
    public bool? GpsLocateAutoCheck   { get; init; }
    public int?  GpsLocateIntervalTime { get; init; }
    public bool? RunGps               { get; init; }

    // LCD gesture
    public bool? LcdGestureOpen       { get; init; }
    public int?  LcdGestureStartHour  { get; init; }
    public int?  LcdGestureEndHour    { get; init; }

    // Auto AF
    public bool? AutoAfOpen           { get; init; }
    public int?  AutoAfInterval       { get; init; }

    // BP calibration
    public double? BpSbpBand          { get; init; }
    public double? BpDbpBand          { get; init; }
    public double? BpSbpMeter         { get; init; }
    public double? BpDbpMeter         { get; init; }

    public DateTime? LastUpdatedAt    { get; init; }
}

public sealed class DeviceConfigPagedResult
{
    public IReadOnlyList<DeviceConfigResponse> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page       { get; init; }
    public int PageSize   { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}
