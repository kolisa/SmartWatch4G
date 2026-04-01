using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of <see cref="IRriQueryService"/>.
/// </summary>
public sealed class RriQueryService : IRriQueryService
{
    private readonly IRriDataRepository _rriRepo;

    public RriQueryService(IRriDataRepository rriRepo)
        => _rriRepo = rriRepo;

    public async Task<IReadOnlyList<RriReadingDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var records = await _rriRepo.GetByDeviceAndDateAsync(deviceId, date, ct)
            .ConfigureAwait(false);

        return records.Select(r => new RriReadingDto
        {
            DeviceId = r.DeviceId ?? string.Empty,
            DataTime = DateTimeUtilities.LocalizeTimestamp(r.DataTime, tzInfo),
            Seq = r.Seq,
            SampleCount = r.SampleCount,
            RriValuesJson = r.RriValuesJson
        }).ToList();
    }
}
