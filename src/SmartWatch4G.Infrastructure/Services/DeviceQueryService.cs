using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of <see cref="IDeviceQueryService"/>.
/// Orchestrates repository calls and maps domain entities to Application DTOs.
/// </summary>
public sealed class DeviceQueryService : IDeviceQueryService
{
    private readonly IDeviceInfoRepository _deviceInfoRepo;
    private readonly IDeviceStatusRepository _statusRepo;

    public DeviceQueryService(
        IDeviceInfoRepository deviceInfoRepo,
        IDeviceStatusRepository statusRepo)
    {
        _deviceInfoRepo = deviceInfoRepo;
        _statusRepo = statusRepo;
    }

    public async Task<IReadOnlyList<DeviceSummaryDto>> GetAllDevicesAsync(
        string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var records = await _deviceInfoRepo.GetAllAsync(ct).ConfigureAwait(false);

        return records.Select(r => new DeviceSummaryDto
        {
            DeviceId = r.DeviceId,
            Model = r.Model,
            Version = r.Version,
            WearingStatus = r.WearingStatus,
            NetworkStatus = r.NetworkStatus,
            UpdatedAt = DateTimeUtilities.LocalizeDateTime(r.UpdatedAt, tzInfo)
        }).ToList();
    }

    public async Task<DeviceDetailDto?> GetDeviceAsync(
        string deviceId, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var r = await _deviceInfoRepo.FindByDeviceIdAsync(deviceId, ct).ConfigureAwait(false);

        if (r is null) return null;

        return new DeviceDetailDto
        {
            DeviceId = r.DeviceId,
            Imsi = r.Imsi,
            Sn = r.Sn,
            Mac = r.Mac,
            NetType = r.NetType,
            NetOperator = r.NetOperator,
            WearingStatus = r.WearingStatus,
            Model = r.Model,
            Version = r.Version,
            Sim1IccId = r.Sim1IccId,
            Sim1CellId = r.Sim1CellId,
            Sim1NetAdhere = r.Sim1NetAdhere,
            NetworkStatus = r.NetworkStatus,
            BandDetail = r.BandDetail,
            RefSignal = r.RefSignal,
            Band = r.Band,
            CommunicationMode = r.CommunicationMode,
            WatchEvent = r.WatchEvent,
            CreatedAt = DateTimeUtilities.LocalizeDateTime(r.CreatedAt, tzInfo),
            UpdatedAt = DateTimeUtilities.LocalizeDateTime(r.UpdatedAt, tzInfo)
        };
    }

    public async Task<IReadOnlyList<DeviceStatusItemDto>> GetStatusByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var records = await _statusRepo.GetByDeviceAndDateAsync(deviceId, date, ct)
            .ConfigureAwait(false);

        return records.Select(r => new DeviceStatusItemDto
        {
            DeviceId = r.DeviceId,
            EventTime = DateTimeUtilities.LocalizeTimestamp(r.EventTime, tzInfo),
            Status = r.Status,
            ReceivedAt = DateTimeUtilities.LocalizeDateTime(r.ReceivedAt, tzInfo)
        }).ToList();
    }

    public async Task<DeviceStatusItemDto?> GetLatestStatusAsync(
        string deviceId, string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var r = await _statusRepo.GetLatestByDeviceAsync(deviceId, ct).ConfigureAwait(false);

        if (r is null) return null;

        return new DeviceStatusItemDto
        {
            DeviceId = r.DeviceId,
            EventTime = DateTimeUtilities.LocalizeTimestamp(r.EventTime, tzInfo),
            Status = r.Status,
            ReceivedAt = DateTimeUtilities.LocalizeDateTime(r.ReceivedAt, tzInfo)
        };
    }

    public async Task<IReadOnlyList<DeviceStatusItemDto>> GetLatestStatusAllDevicesAsync(
        string? tz, CancellationToken ct = default)
    {
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);
        var records = await _statusRepo.GetLatestAllDevicesAsync(ct).ConfigureAwait(false);

        return records.Select(r => new DeviceStatusItemDto
        {
            DeviceId = r.DeviceId,
            EventTime = DateTimeUtilities.LocalizeTimestamp(r.EventTime, tzInfo),
            Status = r.Status,
            ReceivedAt = DateTimeUtilities.LocalizeDateTime(r.ReceivedAt, tzInfo)
        }).ToList();
    }
}
