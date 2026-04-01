using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of <see cref="IAlarmQueryService"/>.
/// </summary>
public sealed class AlarmQueryService : IAlarmQueryService
{
    private readonly IAlarmRepository _alarmRepo;

    public AlarmQueryService(IAlarmRepository alarmRepo)
        => _alarmRepo = alarmRepo;

    public async Task<IReadOnlyList<AlarmEventDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var records = await _alarmRepo.GetByDeviceAndDateAsync(deviceId, date, ct)
            .ConfigureAwait(false);
        return Map(records, tzInfo);
    }

    public async Task<IReadOnlyList<AlarmEventDto>> GetByRangeAsync(
        string deviceId, string from, string to, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var records = await _alarmRepo.GetByDeviceAndTimeRangeAsync(deviceId, from, to, ct)
            .ConfigureAwait(false);
        return Map(records, tzInfo);
    }

    public async Task<AlarmEventDto?> GetLatestAsync(
        string deviceId, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var r = await _alarmRepo.GetLatestByDeviceAsync(deviceId, ct).ConfigureAwait(false);
        return r is null ? null : MapOne(r, tzInfo);
    }

    public async Task<IReadOnlyList<AlarmEventDto>> GetLatestAllDevicesAsync(
        string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var records = await _alarmRepo.GetLatestAllDevicesAsync(ct).ConfigureAwait(false);
        return Map(records, tzInfo);
    }

    public async Task<IReadOnlyList<AlarmEventDto>> GetAllDevicesAndDateAsync(
        string date, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var records = await _alarmRepo.GetAllDevicesAndDateAsync(date, ct).ConfigureAwait(false);
        return Map(records, tzInfo);
    }

    private static IReadOnlyList<AlarmEventDto> Map(
        IEnumerable<Domain.Entities.AlarmEventRecord> records, TimeZoneInfo? tz)
        => records.Select(r => MapOne(r, tz)).ToList();

    private static AlarmEventDto MapOne(Domain.Entities.AlarmEventRecord r, TimeZoneInfo? tz) => new()
    {
        DeviceId = r.DeviceId ?? string.Empty,
        AlarmType = r.AlarmType,
        AlarmTime = DateTimeUtilities.LocalizeTimestamp(r.AlarmTime, tz),
        Value1 = r.Value1,
        Value2 = r.Value2,
        Notes = r.Notes,
        ReceivedAt = DateTimeUtilities.LocalizeDateTime(r.ReceivedAt, tz)
    };
}
