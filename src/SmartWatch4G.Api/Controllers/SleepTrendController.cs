using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Common;
using SmartWatch4G.Domain.Interfaces.Services;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Returns a multi-day sleep trend for a device.
/// Route: GET /api/devices/{deviceId}/sleep/trend?from=yyyy-MM-dd&amp;to=yyyy-MM-dd
/// Days with no sleep data are omitted from the response.
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/sleep/trend")]
public sealed class SleepTrendController : ControllerBase
{
    private readonly ISleepQueryService _sleepService;
    private readonly ILogger<SleepTrendController> _logger;

    public SleepTrendController(
        ISleepQueryService sleepService,
        ILogger<SleepTrendController> logger)
    {
        _sleepService = sleepService;
        _logger = logger;
    }

    /// <summary>
    /// Returns one sleep result per night in the given date range (inclusive).
    /// Dates with no data are silently omitted.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSleepTrendAsync(
        string deviceId,
        [FromQuery] string from,
        [FromQuery] string to,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetSleepTrend — entry, device: {DeviceId}, from: {From}, to: {To}",
            deviceId, from, to);

        if (string.IsNullOrWhiteSpace(deviceId) ||
            !DateTimeUtilities.IsValidDate(from) ||
            !DateTimeUtilities.IsValidDate(to))
        {
            _logger.LogWarning(
                "GetSleepTrend — invalid parameters, device: {DeviceId}, from: {From}, to: {To}",
                deviceId, from, to);
            return BadRequest(new ApiListResponse<SleepTrendItemDto> { ReturnCode = 400 });
        }

        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        ServiceResult<IReadOnlyList<SleepResult>> result;
        try
        {
            result = await _sleepService.GetSleepResultsByDateRangeAsync(deviceId, from, to, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetSleepTrend — service error for device {DeviceId}, {From}→{To}",
                deviceId, from, to);
            return StatusCode(500, new ApiListResponse<SleepTrendItemDto> { ReturnCode = 500 });
        }

        if (result.IsFailure)
        {
            _logger.LogWarning(
                "GetSleepTrend — failure for device {DeviceId}, {From}→{To}: {Error}",
                deviceId, from, to, result.Error);
            return StatusCode(result.ErrorCode == 400 ? 400 : 500,
                new ApiListResponse<SleepTrendItemDto> { ReturnCode = result.ErrorCode });
        }

        var data = result.Value!.Select(r => new SleepTrendItemDto
        {
            DeviceId = r.DeviceId,
            SleepDate = r.SleepDate,
            StartTime = DateTimeUtilities.LocalizeTimestamp(r.StartTime, tzInfo),
            EndTime = DateTimeUtilities.LocalizeTimestamp(r.EndTime, tzInfo),
            TotalSleepMinutes = r.DeepSleepMinutes + r.LightSleepMinutes + r.WeakSleepMinutes + r.EyeMoveSleepMinutes,
            DeepSleep = r.DeepSleepMinutes,
            LightSleep = r.LightSleepMinutes,
            WeakSleep = r.WeakSleepMinutes,
            EyeMoveSleep = r.EyeMoveSleepMinutes,
            Score = r.Score,
            OsahsRisk = r.OsahsRisk,
            Spo2Score = r.Spo2Score,
            SleepHeartRate = r.SleepHeartRate
        }).ToList();

        _logger.LogInformation(
            "GetSleepTrend — exit, device: {DeviceId}, {From}→{To}, nights: {Count}",
            deviceId, from, to, data.Count);

        return Ok(new ApiListResponse<SleepTrendItemDto>
        {
            ReturnCode = 0,
            Count = data.Count,
            Data = data
        });
    }
}
