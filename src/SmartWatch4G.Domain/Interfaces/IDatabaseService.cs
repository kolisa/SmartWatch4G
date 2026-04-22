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
}
