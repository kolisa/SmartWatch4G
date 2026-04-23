using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces;

public interface IDatabaseService
{
    void InsertGpsTrack(string deviceId, string gnssTime, double longitude, double latitude, string locType);

    void UpsertHealthSnapshot(string deviceId, string recordTime,
        int? battery = null, int? rssi = null,
        int? steps = null, double? distance = null, double? calorie = null,
        int? avgHr = null, int? maxHr = null, int? minHr = null,
        int? avgSpo2 = null, int? sbp = null, int? dbp = null, int? fatigue = null);

    void InsertAlarm(string deviceId, string alarmTime, string alarmType, string? details = null);

    void InsertSosEvent(string deviceId, string alarmTime,
        double? lat, double? lon,
        string? callNumber, int? callStatus, string? callStart, string? callEnd);

    void InsertDeviceInfo(string deviceId, string recordedAt,
        string? model, string? version, string? wearingStatus, string? signal, string rawJson);

    void InsertSleepCalculation(string deviceId, string recordDate,
        int completed, string? startTime, string? endTime, int hr, int turnTimes,
        double? respAvg, double? respMax, double? respMin, string? sectionsJson);

    void InsertEcgCalculation(string deviceId, int result, int hr, int effective, int direction);

    void InsertAfCalculation(string deviceId, int result);

    void InsertSpo2Calculation(string deviceId, double spo2Score, int? oshahsRisk);

    void UpsertUserProfile(string deviceId, string name, string surname,
        string? email = null, string? cell = null, string? empNo = null, string? address = null,
        int? companyId = null);

    UserProfile? GetUserProfile(string deviceId);

    IReadOnlyList<UserProfile> GetAllUserProfiles();

    IReadOnlyList<UserProfile> GetUsersByCompanyId(int companyId);

    void DeleteUserProfile(string deviceId);

    void ReactivateUserProfile(string deviceId);

    // ── Company CRUD ──────────────────────────────────────────────────────────

    int CreateCompany(string name, string? registrationNumber, string? contactEmail,
        string? contactPhone, string? address);

    Company? GetCompany(int id);

    IReadOnlyList<Company> GetAllCompanies();

    void UpdateCompany(int id, string name, string? registrationNumber, string? contactEmail,
        string? contactPhone, string? address);

    void DeleteCompany(int id);

    void LinkUserToCompany(string deviceId, int? companyId);

    /// <summary>
    /// Updates user_id and company_id on every data table row that belongs to this device,
    /// pulling the values from the active user_profiles entry.
    /// Returns the total number of rows updated across all tables, or -1 on error.
    /// </summary>
    int BackfillDeviceRecords(string deviceId);

    GnssTrack? GetLatestGnssTrack(string deviceId);

    IReadOnlyList<GnssTrack> GetGnssTracks(string deviceId, System.DateTime? from, System.DateTime? to);

    HealthSnapshot? GetLatestHealthSnapshot(string deviceId);

    int GetActiveWorkerCount();

    int GetActiveWorkerCountByCompany(int companyId);

    IReadOnlyList<UserProfile> GetPagedUserProfiles(int skip, int take);

    IReadOnlyList<UserProfile> GetPagedUserProfilesByCompany(int skip, int take, int companyId);

    int GetRecentAlarmCount(int withinHours);

    int GetRecentSosCount(int withinHours);

    IReadOnlyList<AlarmEvent> GetRecentAlarms(int withinHours, int limit);

    /// <summary>
    /// Returns worker count, alarm count, and SOS count for the given time window
    /// in a single round trip — avoids three separate COUNT queries for the dashboard.
    /// </summary>
    (int TotalWorkers, int AlarmCount, int SosCount) GetDashboardCounts(int withinHours);

    (int TotalWorkers, int AlarmCount, int SosCount) GetDashboardCountsByCompany(int withinHours, int companyId);

    // ── GPS queries ───────────────────────────────────────────────────────────

    /// <summary>Returns paged GPS tracks for all devices in a company, with optional date filter.</summary>
    (IReadOnlyList<(string DeviceId, string? UserName, GnssTrack Track)> Items, int TotalCount)
        GetGnssTracksByCompany(int companyId, System.DateTime? from, System.DateTime? to,
            int skip, int take, string sortDir, bool onlineOnly, bool offlineOnly);

    /// <summary>Returns total online and offline device counts for a company.</summary>
    (int Online, int Offline) GetDeviceStatusCountsByCompany(int companyId, System.Collections.Generic.IReadOnlyList<string> onlineDeviceIds);

    // ── Health queries ────────────────────────────────────────────────────────

    /// <summary>Returns paged health snapshots for a single device with date filters.</summary>
    (IReadOnlyList<HealthSnapshot> Items, int TotalCount)
        GetHealthSnapshotsByDevice(string deviceId, System.DateTime? from, System.DateTime? to,
            int skip, int take, string sortDir);

    /// <summary>Returns paged health snapshots for all devices in a company with date filters.</summary>
    (IReadOnlyList<(string DeviceId, string? UserName, HealthSnapshot Snapshot)> Items, int TotalCount)
        GetHealthSnapshotsByCompany(int companyId, System.DateTime? from, System.DateTime? to,
            int skip, int take, string sortDir);

    /// <summary>Returns per-device health aggregates (avg HR, avg SpO2, total steps) for a company.</summary>
    IReadOnlyList<(string DeviceId, string? UserName, double? AvgHr, double? AvgSpo2,
        double? AvgFatigue, int? MaxHr, int? MinHr, int? TotalSteps, int Count)>
        GetHealthSummaryByCompany(int companyId, System.DateTime? from, System.DateTime? to);

    // ── Device configuration queries ──────────────────────────────────────────

    /// <summary>Returns consolidated command configuration for a single device by joining all device_* setting tables.</summary>
    (string DeviceId, string? UserName, System.DateTime? UpdatedAt,
        // data freq
        bool? GpsAutoCheck, int? GpsIntervalTime, int? PowerMode,
        bool? DataAutoUpload, int? DataUploadInterval, bool? AutoLocate, int? LocateIntervalTime,
        // HR alarm
        bool? HrAlarmOpen, int? HrAlarmHigh, int? HrAlarmLow, int? HrAlarmThreshold, int? HrAlarmInterval,
        // dynamic HR alarm
        bool? DynHrAlarmOpen, int? DynHrAlarmHigh, int? DynHrAlarmLow, int? DynHrAlarmTimeout, int? DynHrAlarmInterval,
        // SpO2 alarm
        bool? Spo2AlarmOpen, int? Spo2AlarmLow,
        // BP alarm
        bool? BpAlarmOpen, int? BpSbpHigh, int? BpSbpBelow, int? BpDbpHigh, int? BpDbpBelow,
        // temp alarm
        bool? TempAlarmOpen, double? TempAlarmHigh, double? TempAlarmLow,
        // fall
        bool? FallCheckEnabled, int? FallThreshold,
        // display
        string? Language, int? HourFormat, string? DateFormat, int? DistanceUnit, int? TemperatureUnit, bool? WearHandRight,
        // intervals
        int? HrInterval, int? OtherInterval,
        // goal
        int? GoalStep, double? GoalDistance, double? GoalCalorie,
        // GPS locate
        bool? GpsLocateAutoCheck, int? GpsLocateIntervalTime, bool? RunGps,
        // LCD
        bool? LcdGestureOpen, int? LcdGestureStartHour, int? LcdGestureEndHour,
        // auto AF
        bool? AutoAfOpen, int? AutoAfInterval,
        // BP adjust
        double? BpSbpBand, double? BpDbpBand, double? BpSbpMeter, double? BpDbpMeter)?
        GetDeviceConfig(string deviceId);

    /// <summary>Returns consolidated command configurations for all devices in a company.</summary>
    IReadOnlyList<(string DeviceId, string? UserName, System.DateTime? UpdatedAt,
        bool? GpsAutoCheck, int? GpsIntervalTime, int? PowerMode,
        bool? DataAutoUpload, int? DataUploadInterval, bool? AutoLocate, int? LocateIntervalTime,
        bool? HrAlarmOpen, int? HrAlarmHigh, int? HrAlarmLow, int? HrAlarmThreshold, int? HrAlarmInterval,
        bool? DynHrAlarmOpen, int? DynHrAlarmHigh, int? DynHrAlarmLow, int? DynHrAlarmTimeout, int? DynHrAlarmInterval,
        bool? Spo2AlarmOpen, int? Spo2AlarmLow,
        bool? BpAlarmOpen, int? BpSbpHigh, int? BpSbpBelow, int? BpDbpHigh, int? BpDbpBelow,
        bool? TempAlarmOpen, double? TempAlarmHigh, double? TempAlarmLow,
        bool? FallCheckEnabled, int? FallThreshold,
        string? Language, int? HourFormat, string? DateFormat, int? DistanceUnit, int? TemperatureUnit, bool? WearHandRight,
        int? HrInterval, int? OtherInterval,
        int? GoalStep, double? GoalDistance, double? GoalCalorie,
        bool? GpsLocateAutoCheck, int? GpsLocateIntervalTime, bool? RunGps,
        bool? LcdGestureOpen, int? LcdGestureStartHour, int? LcdGestureEndHour,
        bool? AutoAfOpen, int? AutoAfInterval,
        double? BpSbpBand, double? BpDbpBand, double? BpSbpMeter, double? BpDbpMeter)>
        GetDeviceConfigsByCompany(int companyId, int skip, int take);

    int GetDeviceConfigCountByCompany(int companyId);
}
