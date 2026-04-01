using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Application.Utilities;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only ECG waveform endpoints consumed by mobile and web applications.
/// Route: GET /api/devices/{deviceId}/ecg?date=
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/ecg")]
public sealed class EcgQueryController : ControllerBase
{
    private readonly IEcgQueryService _ecgService;
    private readonly ILogger<EcgQueryController> _logger;

    public EcgQueryController(
        IEcgQueryService ecgService,
        ILogger<EcgQueryController> logger)
    {
        _ecgService = ecgService;
        _logger = logger;
    }

    /// <summary>Returns ECG waveform records for the given device and date.</summary>
    [HttpGet]
    public async Task<IActionResult> GetEcgAsync(
        string deviceId,
        [FromQuery] string date,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetEcg — entry, device: {DeviceId}, date: {Date}", deviceId, date);

        if (string.IsNullOrWhiteSpace(deviceId) || !DateTimeUtilities.IsValidDate(date))
        {
            _logger.LogWarning(
                "GetEcg — invalid parameters, device: {DeviceId}, date: {Date}", deviceId, date);
            return BadRequest(new ApiListResponse<EcgRecordDto> { ReturnCode = 400 });
        }

        IReadOnlyList<EcgRecordDto> data;
        try
        {
            data = await _ecgService.GetByDateAsync(deviceId, date, tz, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetEcg — DB read failed for device {DeviceId}, date {Date}", deviceId, date);
            return StatusCode(500, new ApiListResponse<EcgRecordDto> { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "GetEcg — exit, device: {DeviceId}, date: {Date}, count: {Count}",
            deviceId, date, data.Count);

        return Ok(new ApiListResponse<EcgRecordDto>
        {
            ReturnCode = 0,
            Count = data.Count,
            Data = data
        });
    }
}
