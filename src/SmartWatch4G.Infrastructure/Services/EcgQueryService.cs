using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of <see cref="IEcgQueryService"/>.
/// </summary>
public sealed class EcgQueryService : IEcgQueryService
{
    private readonly IHealthDataRepository _healthRepo;

    public EcgQueryService(IHealthDataRepository healthRepo)
        => _healthRepo = healthRepo;

    public async Task<IReadOnlyList<EcgRecordDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var records = await _healthRepo.GetEcgByDeviceAndDateAsync(deviceId, date, ct)
            .ConfigureAwait(false);

        return records.Select(r => new EcgRecordDto
        {
            DeviceId = r.DeviceId ?? string.Empty,
            DataTime = DateTimeUtilities.LocalizeTimestamp(r.DataTime, tzInfo),
            Seq = r.Seq,
            SampleCount = r.SampleCount,
            RawDataBase64 = r.RawDataBase64
        }).ToList();
    }
}
