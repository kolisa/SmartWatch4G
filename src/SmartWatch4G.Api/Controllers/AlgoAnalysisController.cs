using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Triggers on-demand calls to the iwown algorithm service for ECG rhythm classification,
/// AF detection, SpO2 OSAHS risk, and Parkinson ACC scoring.
///
/// Routes:
///   GET /api/v1/devices/{deviceId}/analysis/ecg?dataTime=
///   GET /api/v1/devices/{deviceId}/analysis/af?date=
///   GET /api/v1/devices/{deviceId}/analysis/spo2?date=
///   GET /api/v1/devices/{deviceId}/analysis/parkinson?date=
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("dashboard-api")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/analysis")]
public sealed class AlgoAnalysisController : ControllerBase
{
    private readonly IAlgoAnalysisService _algoService;
    private readonly ILogger<AlgoAnalysisController> _logger;
    private readonly IDateTimeService _dt;

    public AlgoAnalysisController(
        IAlgoAnalysisService algoService,
        ILogger<AlgoAnalysisController> logger,
        IDateTimeService dt)
    {
        _algoService = algoService;
        _logger = logger;
        _dt = dt;
    }

    /// <summary>
    /// Classifies ECG rhythm for a specific measurement.
    /// Loads all ECG chunks stored for (deviceId, dataTime) and calls the iwown ECG algorithm.
    /// </summary>
    [HttpGet("ecg")]
    public async Task<IActionResult> AnalyseEcgAsync(
        string deviceId,
        [FromQuery] string dataTime,
        CancellationToken ct)
    {
        _logger.LogInformation("AnalyseEcg — entry, device: {DeviceId}, dataTime: {DataTime}", deviceId, dataTime);

        if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(dataTime))
            return BadRequest(new ApiItemResponse<RhythmAnalysisDto> { ReturnCode = 400 });

        RhythmAnalysisDto? result;
        try
        {
            result = await _algoService.AnalyseEcgAsync(deviceId, dataTime, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AnalyseEcg — failed for device {DeviceId}, dataTime {DataTime}", deviceId, dataTime);
            return StatusCode(500, new ApiItemResponse<RhythmAnalysisDto> { ReturnCode = 500 });
        }

        if (result is null)
            return Ok(new ApiItemResponse<RhythmAnalysisDto> { ReturnCode = 10404 });

        _logger.LogInformation("AnalyseEcg — exit, device: {DeviceId}, result: {Result}", deviceId, result.Result);
        return Ok(new ApiItemResponse<RhythmAnalysisDto> { ReturnCode = 0, Data = result });
    }

    /// <summary>
    /// Detects atrial fibrillation from all RRI records stored for (deviceId, date).
    /// </summary>
    [HttpGet("af")]
    public async Task<IActionResult> AnalyseAfAsync(
        string deviceId,
        [FromQuery] string date,
        CancellationToken ct)
    {
        _logger.LogInformation("AnalyseAf — entry, device: {DeviceId}, date: {Date}", deviceId, date);

        if (string.IsNullOrWhiteSpace(deviceId) || !_dt.IsValidDate(date))
            return BadRequest(new ApiItemResponse<RhythmAnalysisDto> { ReturnCode = 400 });

        RhythmAnalysisDto? result;
        try
        {
            result = await _algoService.AnalyseAfAsync(deviceId, date, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AnalyseAf — failed for device {DeviceId}, date {Date}", deviceId, date);
            return StatusCode(500, new ApiItemResponse<RhythmAnalysisDto> { ReturnCode = 500 });
        }

        if (result is null)
            return Ok(new ApiItemResponse<RhythmAnalysisDto> { ReturnCode = 10404 });

        _logger.LogInformation("AnalyseAf — exit, device: {DeviceId}, result: {Result}", deviceId, result.Result);
        return Ok(new ApiItemResponse<RhythmAnalysisDto> { ReturnCode = 0, Data = result });
    }

    /// <summary>
    /// Scores continuous SpO2 data for OSAHS risk for all SpO2 records on (deviceId, date).
    /// </summary>
    [HttpGet("spo2")]
    public async Task<IActionResult> AnalyseSpo2Async(
        string deviceId,
        [FromQuery] string date,
        CancellationToken ct)
    {
        _logger.LogInformation("AnalyseSpo2 — entry, device: {DeviceId}, date: {Date}", deviceId, date);

        if (string.IsNullOrWhiteSpace(deviceId) || !_dt.IsValidDate(date))
            return BadRequest(new ApiItemResponse<Spo2AnalysisDto> { ReturnCode = 400 });

        Spo2AnalysisDto? result;
        try
        {
            result = await _algoService.AnalyseSpo2Async(deviceId, date, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AnalyseSpo2 — failed for device {DeviceId}, date {Date}", deviceId, date);
            return StatusCode(500, new ApiItemResponse<Spo2AnalysisDto> { ReturnCode = 500 });
        }

        if (result is null)
            return Ok(new ApiItemResponse<Spo2AnalysisDto> { ReturnCode = 10404 });

        _logger.LogInformation("AnalyseSpo2 — exit, device: {DeviceId}, score: {Score}, risk: {Risk}",
            deviceId, result.Spo2Score, result.OsahsRisk);
        return Ok(new ApiItemResponse<Spo2AnalysisDto> { ReturnCode = 0, Data = result });
    }

    /// <summary>
    /// Scores Parkinson tremor/activity from all ACC records stored for (deviceId, date).
    /// </summary>
    [HttpGet("parkinson")]
    public async Task<IActionResult> AnalyseParkinsonAsync(
        string deviceId,
        [FromQuery] string date,
        CancellationToken ct)
    {
        _logger.LogInformation("AnalyseParkinson — entry, device: {DeviceId}, date: {Date}", deviceId, date);

        if (string.IsNullOrWhiteSpace(deviceId) || !_dt.IsValidDate(date))
            return BadRequest(new ApiItemResponse<ParkinsonAnalysisDto> { ReturnCode = 400 });

        ParkinsonAnalysisDto? result;
        try
        {
            result = await _algoService.AnalyseParkinsonAsync(deviceId, date, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AnalyseParkinson — failed for device {DeviceId}, date {Date}", deviceId, date);
            return StatusCode(500, new ApiItemResponse<ParkinsonAnalysisDto> { ReturnCode = 500 });
        }

        if (result is null)
            return Ok(new ApiItemResponse<ParkinsonAnalysisDto> { ReturnCode = 10404 });

        _logger.LogInformation("AnalyseParkinson — exit, device: {DeviceId}, tremor: {T}, activity: {A}",
            deviceId, result.TremorScore, result.ActivityScore);
        return Ok(new ApiItemResponse<ParkinsonAnalysisDto> { ReturnCode = 0, Data = result });
    }
}
