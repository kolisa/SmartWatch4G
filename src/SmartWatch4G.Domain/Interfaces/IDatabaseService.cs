using SmartWatch4G.Domain.Entities;

namespace SmartWatch4G.Domain.Interfaces;

public interface IDatabaseService
{
    Task InsertGpsTrack(string deviceId, string gnssTime, double longitude, double latitude, string locType);

    Task UpsertHealthSnapshot(string deviceId, string recordTime,
        int? battery = null, int? rssi = null,
        int? steps = null, double? distance = null, double? calorie = null,
        int? avgHr = null, int? maxHr = null, int? minHr = null,
        int? avgSpo2 = null, int? sbp = null, int? dbp = null, int? fatigue = null,
        double? bodyTempEvi = null, int? bodyTempEsti = null, int? tempType = null,
        int? bpBpm = null, double? bloodPotassium = null, double? bloodSugar = null,
        double? biozR = null, double? biozX = null, double? biozFat = null,
        double? biozBmi = null, int? biozType = null,
        double? breathRate = null, int? moodLevel = null);

    Task InsertAlarm(string deviceId, string alarmTime, string alarmType, string? details = null);

    Task InsertSosEvent(string deviceId, string alarmTime,
        double? lat, double? lon,
        string? callNumber, int? callStatus, string? callStart, string? callEnd);

    Task InsertDeviceInfo(string deviceId, string recordedAt,
        string? model, string? version, string? wearingStatus, string? signal, string rawJson);

    Task InsertSleepCalculation(string deviceId, string recordDate,
        int completed, string? startTime, string? endTime, int hr, int turnTimes,
        double? respAvg, double? respMax, double? respMin, string? sectionsJson,
        int? deepSleep = null, int? lightSleep = null, int? weakSleep = null, int? eyemoveSleep = null);

    Task<SleepCalculation?> GetSleepCalculation(string deviceId, string sleepDate);

    Task InsertEcgWaveform(string deviceId, string recordedAt, int sampleCount, string rawDataJson);
    Task InsertPpgWaveform(string deviceId, string recordedAt, int sampleCount, string rawDataJson);
    Task InsertAccWaveform(string deviceId, string recordedAt, int sampleCount, string? accXBase64, string? accYBase64, string? accZBase64);
    Task InsertRriWaveform(string deviceId, string recordedAt, int sampleCount, string rawDataJson);
    Task InsertSpo2Waveform(string deviceId, string recordedAt, string readingsJson);
    Task InsertMultiLeadsEcgWaveform(string deviceId, string recordedAt, int channels, int byteLen, string rawBase64);
    Task InsertThirdPartyReading(string deviceId, string macAddr, string? devName, string readingType,
        string? recordedAt, double? sbp, double? dbp, double? hr, double? pulse,
        double? weight, double? impedance, double? bodyFatPct,
        double? spo2, double? pi, double? bodyTemp, double? value);

    Task InsertEcgCalculation(string deviceId, int result, int hr, int effective, int direction);

    Task InsertAfCalculation(string deviceId, int result);

    Task InsertSpo2Calculation(string deviceId, double spo2Score, int? oshahsRisk);

    Task UpsertUserProfile(string deviceId, string name, string surname,
        string? email = null, string? cell = null, string? empNo = null, string? address = null,
        int? companyId = null);

    Task<UserProfile?> GetUserProfile(string deviceId);

    Task<IReadOnlyList<UserProfile>> GetAllUserProfiles();

    Task<IReadOnlyList<UserProfile>> GetUsersByCompanyId(int companyId);

    Task DeleteUserProfile(string deviceId);

    Task ReactivateUserProfile(string deviceId);

    // ── Company CRUD ──────────────────────────────────────────────────────────

    Task<int> CreateCompany(string name, string? registrationNumber, string? contactEmail,
        string? contactPhone, string? address);

    Task<Company?> GetCompany(int id);

    Task<IReadOnlyList<Company>> GetAllCompanies();

    Task UpdateCompany(int id, string name, string? registrationNumber, string? contactEmail,
        string? contactPhone, string? address);

    Task DeleteCompany(int id);

    Task LinkUserToCompany(string deviceId, int? companyId);

    /// <summary>
    /// Updates user_id and company_id on every data table row that belongs to this device,
    /// pulling the values from the active user_profiles entry.
    /// Returns the total number of rows updated across all tables, or -1 on error.
    /// </summary>
    Task<int> BackfillDeviceRecords(string deviceId);

    Task<GnssTrack?> GetLatestGnssTrack(string deviceId);

    Task<IReadOnlyList<GnssTrack>> GetGnssTracks(string deviceId, System.DateTime? from, System.DateTime? to);

    Task<HealthSnapshot?> GetLatestHealthSnapshot(string deviceId);

    Task<int> GetActiveWorkerCount();

    Task<int> GetActiveWorkerCountByCompany(int companyId);

    Task<IReadOnlyList<UserProfile>> GetPagedUserProfiles(int skip, int take);

    /// <summary>
    /// Returns paged user profiles combined with their latest health snapshot and GPS track
    /// in a single round trip using OUTER APPLY. Avoids N+1 queries per device.
    /// </summary>
    Task<(IReadOnlyList<UserProfileWithData> Items, int TotalCount)> GetPagedUserProfilesWithData(
        int skip, int take, int? companyId);

    Task<IReadOnlyList<UserProfile>> GetPagedUserProfilesByCompany(int skip, int take, int companyId);

    Task<int> GetRecentAlarmCount(int withinHours);

    Task<int> GetRecentSosCount(int withinHours);

    Task<IReadOnlyList<AlarmEvent>> GetRecentAlarms(int withinHours, int limit);

    /// <summary>
    /// Returns dashboard stats in a single round trip:
    /// total workers, SOS count, HR alert count, and devices tracked on map (GPS in last 24 h).
    /// Online/offline counts are supplied by the caller from IDeviceStatusCache.
    /// </summary>
    Task<(int TotalWorkers, int SosCount, int HrAlertCount, int TrackedOnMap)> GetDashboardCounts(int withinHours, int? companyId = null);

    // ── GPS queries ───────────────────────────────────────────────────────────

    /// <summary>Returns paged GPS tracks for all devices in a company, with optional date filter.</summary>
    Task<(IReadOnlyList<(string DeviceId, string? UserName, GnssTrack Track)> Items, int TotalCount)>
        GetGnssTracksByCompany(int companyId, System.DateTime? from, System.DateTime? to,
            int skip, int take, string sortDir, bool onlineOnly, bool offlineOnly);

    /// <summary>Returns total online and offline device counts for a company.</summary>
    Task<(int Online, int Offline)> GetDeviceStatusCountsByCompany(int companyId, System.Collections.Generic.IReadOnlyList<string> onlineDeviceIds);

    // ── Health queries ────────────────────────────────────────────────────────

    /// <summary>Returns paged health snapshots for a single device with date filters.</summary>
    Task<(IReadOnlyList<HealthSnapshot> Items, int TotalCount)>
        GetHealthSnapshotsByDevice(string deviceId, System.DateTime? from, System.DateTime? to,
            int skip, int take, string sortDir);

    /// <summary>Returns paged health snapshots for all devices in a company with date filters.</summary>
    Task<(IReadOnlyList<(string DeviceId, string? UserName, HealthSnapshot Snapshot)> Items, int TotalCount)>
        GetHealthSnapshotsByCompany(int companyId, System.DateTime? from, System.DateTime? to,
            int skip, int take, string sortDir);

    /// <summary>Returns per-device health aggregates (avg HR, avg SpO2, total steps) for a company.</summary>
    Task<IReadOnlyList<(string DeviceId, string? UserName, double? AvgHr, double? AvgSpo2,
        double? AvgFatigue, int? MaxHr, int? MinHr, int? TotalSteps, int Count)>>
        GetHealthSummaryByCompany(int companyId, System.DateTime? from, System.DateTime? to);

    // ── Device configuration queries ──────────────────────────────────────────

    /// <summary>Returns consolidated command configuration for a single device by joining all device_* setting tables.</summary>
    Task<(string DeviceId, string? UserName, System.DateTime? UpdatedAt,
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
        double? BpSbpBand, double? BpDbpBand, double? BpSbpMeter, double? BpDbpMeter)?>
        GetDeviceConfig(string deviceId);

    /// <summary>Returns consolidated command configurations for all devices in a company.</summary>
    Task<IReadOnlyList<(string DeviceId, string? UserName, System.DateTime? UpdatedAt,
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
        double? BpSbpBand, double? BpDbpBand, double? BpSbpMeter, double? BpDbpMeter)>>
        GetDeviceConfigsByCompany(int companyId, int skip, int take);

    Task<int> GetDeviceConfigCountByCompany(int companyId);

    // ── Audit log ─────────────────────────────────────────────────────────────

    Task<(IReadOnlyList<AuditEntry> Items, int TotalCount)> GetAuditLog(
        string? deviceId = null,
        string? action = null,
        string? tableName = null,
        System.DateTime? from = null,
        System.DateTime? to = null,
        int skip = 0,
        int take = 50);
}
