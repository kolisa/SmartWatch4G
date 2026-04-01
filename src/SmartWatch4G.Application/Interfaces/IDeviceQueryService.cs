using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Application.Interfaces;

/// <summary>
/// Application service for querying device registration and status data.
/// Controllers depend on this interface — no repository or DbContext enters the API layer.
/// </summary>
public interface IDeviceQueryService
{
    Task<IReadOnlyList<DeviceSummaryDto>> GetAllDevicesAsync(
        string? tz, CancellationToken ct = default);

    Task<DeviceDetailDto?> GetDeviceAsync(
        string deviceId, string? tz, CancellationToken ct = default);

    Task<IReadOnlyList<DeviceStatusItemDto>> GetStatusByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default);

    Task<DeviceStatusItemDto?> GetLatestStatusAsync(
        string deviceId, string? tz, CancellationToken ct = default);

    Task<IReadOnlyList<DeviceStatusItemDto>> GetLatestStatusAllDevicesAsync(
        string? tz, CancellationToken ct = default);
}
