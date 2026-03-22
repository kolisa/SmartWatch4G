using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

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
    private readonly IHealthDataRepository _healthRepo;
    private readonly ILogger<EcgQueryController> _logger;

    public EcgQueryController(
        IHealthDataRepository healthRepo,
        ILogger<EcgQueryController> logger)
    {
        _healthRepo = healthRepo;
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
        TimeZoneInfo? tzInfo = DateTimeUtilities.TryGetTimeZone(tz);

        if (string.IsNullOrWhiteSpace(deviceId) || !DateTimeUtilities.IsValidDate(date))
        {
            _logger.LogWarning(
                "GetEcg — invalid parameters, device: {DeviceId}, date: {Date}", deviceId, date);
            return BadRequest(new ApiListResponse<EcgRecordDto> { ReturnCode = 400 });
        }

        IReadOnlyList<EcgDataRecord> records;
        try
        {
            records = await _healthRepo.GetEcgByDeviceAndDateAsync(deviceId, date, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetEcg — DB read failed for device {DeviceId}, date {Date}", deviceId, date);
            return StatusCode(500, new ApiListResponse<EcgRecordDto> { ReturnCode = 500 });
        }

        var data = records.Select(r => new EcgRecordDto
        {
            DeviceId = r.DeviceId ?? string.Empty,
            DataTime = DateTimeUtilities.LocalizeTimestamp(r.DataTime, tzInfo),
            Seq = r.Seq,
            SampleCount = r.SampleCount,
            RawDataBase64 = r.RawDataBase64
        }).ToList();

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
