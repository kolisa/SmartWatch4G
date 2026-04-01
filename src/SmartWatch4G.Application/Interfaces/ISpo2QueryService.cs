using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Application.Interfaces;

/// <summary>
/// Application service for querying SpO2 (blood-oxygen) readings.
/// Controllers depend on this interface — no repository or DbContext enters the API layer.
/// </summary>
public interface ISpo2QueryService
{
    Task<IReadOnlyList<Spo2ReadingDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default);

    Task<IReadOnlyList<Spo2ReadingDto>> GetByRangeAsync(
        string deviceId, string from, string to, string? tz, CancellationToken ct = default);

    Task<Spo2ReadingDto?> GetLatestAsync(
        string deviceId, string? tz, CancellationToken ct = default);

    Task<IReadOnlyList<Spo2ReadingDto>> GetLatestAllDevicesAsync(
        string? tz, CancellationToken ct = default);
}
