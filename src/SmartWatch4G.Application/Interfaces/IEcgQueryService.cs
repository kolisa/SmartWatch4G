using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Application.Interfaces;

/// <summary>
/// Application service for querying ECG waveform records.
/// Controllers depend on this interface — no repository or DbContext enters the API layer.
/// </summary>
public interface IEcgQueryService
{
    Task<IReadOnlyList<EcgRecordDto>> GetByDateAsync(
        string deviceId, string date, string? tz, CancellationToken ct = default);
}
