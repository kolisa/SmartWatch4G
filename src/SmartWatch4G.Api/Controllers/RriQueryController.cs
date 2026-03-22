using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Read-only RRI (R-to-R interval / HRV) endpoints consumed by mobile and web applications.
/// Route: GET /api/devices/{deviceId}/rri?date=
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/rri")]
public sealed class RriQueryController : ControllerBase
{
    private readonly IRriDataRepository _rriRepo;
    private readonly ILogger<RriQueryController> _logger;

    public RriQueryController(
        IRriDataRepository rriRepo,
        ILogger<RriQueryController> logger)
    {
        _rriRepo = rriRepo;
        _logger = logger;
    }

    /// <summary>Returns RRI readings for the given device and date.</summary>
    [HttpGet]
    public async Task<IActionResult> GetRriAsync(
        string deviceId,
        [FromQuery] string date,
        [FromQuery] string? tz,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetRri — entry, device: {DeviceId}, date: {Date}", deviceId, date);
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        if (string.IsNullOrWhiteSpace(deviceId) || !DateTimeUtilities.IsValidDate(date))
        {
            _logger.LogWarning(
                "GetRri — invalid parameters, device: {DeviceId}, date: {Date}", deviceId, date);
            return BadRequest(new ApiListResponse<RriReadingDto> { ReturnCode = 400 });
        }

        IReadOnlyList<RriDataRecord> records;
        try
        {
            records = await _rriRepo.GetByDeviceAndDateAsync(deviceId, date, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetRri — DB read failed for device {DeviceId}, date {Date}", deviceId, date);
            return StatusCode(500, new ApiListResponse<RriReadingDto> { ReturnCode = 500 });
        }

        var data = records.Select(r => new RriReadingDto
        {
            DeviceId = r.DeviceId ?? string.Empty,
            DataTime = DateTimeUtilities.LocalizeTimestamp(r.DataTime, tzInfo),
            Seq = r.Seq,
            SampleCount = r.SampleCount,
            RriValuesJson = r.RriValuesJson
        }).ToList();

        _logger.LogInformation(
            "GetRri — exit, device: {DeviceId}, date: {Date}, count: {Count}",
            deviceId, date, data.Count);

        return Ok(new ApiListResponse<RriReadingDto>
        {
            ReturnCode = 0,
            Count = data.Count,
            Data = data
        });
    }
}
