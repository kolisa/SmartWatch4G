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

    void DeleteUserProfile(string deviceId);

    // ── Company CRUD ──────────────────────────────────────────────────────────

    int CreateCompany(string name, string? registrationNumber, string? contactEmail,
        string? contactPhone, string? address);

    Company? GetCompany(int id);

    IReadOnlyList<Company> GetAllCompanies();

    void UpdateCompany(int id, string name, string? registrationNumber, string? contactEmail,
        string? contactPhone, string? address);

    void DeleteCompany(int id);

    void LinkUserToCompany(string deviceId, int? companyId);

    GnssTrack? GetLatestGnssTrack(string deviceId);

    IReadOnlyList<GnssTrack> GetGnssTracks(string deviceId, System.DateTime? from, System.DateTime? to);

    HealthSnapshot? GetLatestHealthSnapshot(string deviceId);

    int GetActiveWorkerCount();

    IReadOnlyList<UserProfile> GetPagedUserProfiles(int skip, int take);

    int GetRecentAlarmCount(int withinHours);

    int GetRecentSosCount(int withinHours);

    IReadOnlyList<AlarmEvent> GetRecentAlarms(int withinHours, int limit);

    /// <summary>
    /// Returns worker count, alarm count, and SOS count for the given time window
    /// in a single round trip — avoids three separate COUNT queries for the dashboard.
    /// </summary>
    (int TotalWorkers, int AlarmCount, int SosCount) GetDashboardCounts(int withinHours);
}
