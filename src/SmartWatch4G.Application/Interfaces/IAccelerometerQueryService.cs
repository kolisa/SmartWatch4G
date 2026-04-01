using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Application.Interfaces;

/// <summary>
/// Application service for querying accelerometer data.
/// Controllers depend on this interface — no repository or DbContext enters the API layer.
/// </summary>
public interface IAccelerometerQueryService
{
    Task<IReadOnlyList<AccelerometerReadingDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default);
}
