using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of <see cref="IAccelerometerQueryService"/>.
/// </summary>
public sealed class AccelerometerQueryService : IAccelerometerQueryService
{
    private readonly IAccDataRepository _accRepo;
    private readonly IDateTimeService _dt;

    public AccelerometerQueryService(IAccDataRepository accRepo, IDateTimeService dt)
    {
        _accRepo = accRepo;
        _dt = dt;
    }

    public async Task<IReadOnlyList<AccelerometerReadingDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = _dt.TryGetTimeZone(tz);
        (string from, string to) = _dt.ToDayRange(date);
        var records = await _accRepo.GetByDeviceAndDateRangeAsync(deviceId, from, to, ct)
            .ConfigureAwait(false);

        return records.Select(r => new AccelerometerReadingDto
        {
            DeviceId = r.DeviceId ?? string.Empty,
            DataTime = _dt.LocalizeTimestamp(r.DataTime, tzInfo),
            SampleCount = r.SampleCount,
            XValuesJson = r.XValuesJson,
            YValuesJson = r.YValuesJson,
            ZValuesJson = r.ZValuesJson
        }).ToList();
    }
}
