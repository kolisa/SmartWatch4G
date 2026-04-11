using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of <see cref="ICallLogQueryService"/>.
/// </summary>
public sealed class CallLogQueryService : ICallLogQueryService
{
    private readonly ICallLogRepository _callLogRepo;
    private readonly IDateTimeService _dt;

    public CallLogQueryService(ICallLogRepository callLogRepo, IDateTimeService dt)
    {
        _callLogRepo = callLogRepo;
        _dt = dt;
    }

    public async Task<IReadOnlyList<CallLogItemDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = _dt.TryGetTimeZone(tz);
        var records = await _callLogRepo.GetByDeviceAndDateAsync(deviceId, date, ct)
            .ConfigureAwait(false);
        return Map(records, tzInfo);
    }

    public async Task<IReadOnlyList<CallLogItemDto>> GetByRangeAsync(
        string deviceId, string from, string to, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = _dt.TryGetTimeZone(tz);
        var records = await _callLogRepo.GetByDeviceAndTimeRangeAsync(deviceId, from, to, ct)
            .ConfigureAwait(false);
        return Map(records, tzInfo);
    }

    public async Task<IReadOnlyList<CallLogItemDto>> GetAllDevicesAndDateAsync(
        string date, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = _dt.TryGetTimeZone(tz);
        var records = await _callLogRepo.GetAllDevicesAndDateAsync(date, ct).ConfigureAwait(false);
        return Map(records, tzInfo);
    }

    private IReadOnlyList<CallLogItemDto> Map(
        IEnumerable<Domain.Entities.CallLogRecord> records, TimeZoneInfo? tz)
        => records.Select(r => MapOne(r, tz)).ToList();

    private CallLogItemDto MapOne(Domain.Entities.CallLogRecord r, TimeZoneInfo? tz) => new()
    {
        DeviceId = r.DeviceId,
        CallStatus = r.CallStatus,
        CallNumber = r.CallNumber,
        StartTime = _dt.LocalizeTimestamp(r.StartTime, tz),
        EndTime = _dt.LocalizeTimestamp(r.EndTime, tz),
        IsSosAlarm = r.IsSosAlarm,
        AlarmTime = _dt.LocalizeTimestamp(r.AlarmTime, tz),
        AlarmLat = r.AlarmLat,
        AlarmLon = r.AlarmLon,
        ReceivedAt = _dt.LocalizeDateTime(r.ReceivedAt, tz)
    };
}
