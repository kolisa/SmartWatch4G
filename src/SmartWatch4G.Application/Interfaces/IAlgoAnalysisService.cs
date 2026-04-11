using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Application.Interfaces;

/// <summary>
/// Application service that calls the iwown algo service for ECG rhythm classification,
/// AF detection, SpO2 OSAHS risk, and Parkinson analysis.
/// </summary>
public interface IAlgoAnalysisService
{
    /// <summary>
    /// Merges all ECG chunks for (deviceId, dataTime) into a flat list and
    /// calls POST /calculation/ecg on the iwown algo service.
    /// </summary>
    Task<RhythmAnalysisDto?> AnalyseEcgAsync(string deviceId, string dataTime, CancellationToken ct = default);

    /// <summary>
    /// Merges all RRI records for (deviceId, date) into a flat list and
    /// calls POST /calculation/af on the iwown algo service.
    /// </summary>
    Task<RhythmAnalysisDto?> AnalyseAfAsync(string deviceId, string date, CancellationToken ct = default);

    /// <summary>
    /// Merges all SpO2 samples for (deviceId, date) into a flat list and
    /// calls POST /calculation/spo2 on the iwown algo service.
    /// </summary>
    Task<Spo2AnalysisDto?> AnalyseSpo2Async(string deviceId, string date, CancellationToken ct = default);

    /// <summary>
    /// Merges all ACC records for (deviceId, date) and calls
    /// POST /calculation/parkinson/acc on the iwown algo service.
    /// </summary>
    Task<ParkinsonAnalysisDto?> AnalyseParkinsonAsync(string deviceId, string date, CancellationToken ct = default);
}
