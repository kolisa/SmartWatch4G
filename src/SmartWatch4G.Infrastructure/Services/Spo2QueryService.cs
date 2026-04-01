using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of <see cref="ISpo2QueryService"/>.
/// </summary>
public sealed class Spo2QueryService : ISpo2QueryService
{
    private readonly ISpo2DataRepository _spo2Repo;

    public Spo2QueryService(ISpo2DataRepository spo2Repo)
        => _spo2Repo = spo2Repo;

    public async Task<IReadOnlyList<Spo2ReadingDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        (string from, string to) = DateTimeUtilities.ToDayRange(date);
        var records = await _spo2Repo.GetByDeviceAndDateRangeAsync(deviceId, from, to, ct)
            .ConfigureAwait(false);
        return Map(records, tzInfo);
    }

    public async Task<IReadOnlyList<Spo2ReadingDto>> GetByRangeAsync(
        string deviceId, string from, string to, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var records = await _spo2Repo.GetByDeviceAndDateRangeAsync(deviceId, from, to, ct)
            .ConfigureAwait(false);
        return Map(records, tzInfo);
    }

    public async Task<Spo2ReadingDto?> GetLatestAsync(
        string deviceId, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var r = await _spo2Repo.GetLatestByDeviceAsync(deviceId, ct).ConfigureAwait(false);
        return r is null ? null : MapOne(r, tzInfo);
    }

    public async Task<IReadOnlyList<Spo2ReadingDto>> GetLatestAllDevicesAsync(
        string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var records = await _spo2Repo.GetLatestAllDevicesAsync(ct).ConfigureAwait(false);
        return Map(records, tzInfo);
    }

    private static IReadOnlyList<Spo2ReadingDto> Map(
        IEnumerable<Domain.Entities.Spo2DataRecord> records, TimeZoneInfo? tz)
        => records.Select(r => MapOne(r, tz)).ToList();

    private static Spo2ReadingDto MapOne(Domain.Entities.Spo2DataRecord r, TimeZoneInfo? tz) => new()
    {
        DeviceId = r.DeviceId ?? string.Empty,
        DataTime = DateTimeUtilities.LocalizeTimestamp(r.DataTime, tz),
        Spo2 = r.Spo2,
        HeartRate = r.HeartRate,
        Perfusion = r.Perfusion,
        Touch = r.Touch
    };
}
