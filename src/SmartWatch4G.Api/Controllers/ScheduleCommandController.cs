using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Interfaces.Services;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// App-facing endpoints to manage clock alarms and sedentary reminders on a device
/// via the iwown entservice.
///
/// Routes:
///   PUT    /api/v1/devices/{deviceId}/clock-alarms  — set clock alarms (max 5)
///   DELETE /api/v1/devices/{deviceId}/clock-alarms  — clear all clock alarms
///   PUT    /api/v1/devices/{deviceId}/sedentary     — set sedentary reminders (max 3)
///   DELETE /api/v1/devices/{deviceId}/sedentary     — clear all sedentary reminders
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("dashboard-api")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}")]
public sealed class ScheduleCommandController : ControllerBase
{
    private readonly IWownCommandClient _commandClient;
    private readonly ILogger<ScheduleCommandController> _logger;

    public ScheduleCommandController(
        IWownCommandClient commandClient,
        ILogger<ScheduleCommandController> logger)
    {
        _commandClient = commandClient;
        _logger = logger;
    }

    // ── Clock alarms ──────────────────────────────────────────────────────────

    /// <summary>Set clock alarms on a device (max 5 entries).</summary>
    [HttpPut("clock-alarms")]
    public async Task<IActionResult> SetClockAlarmsAsync(
        string deviceId,
        [FromBody] SetClockAlarmsRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "SetClockAlarms — entry, device: {DeviceId}, count: {Count}",
            deviceId, req.Alarms.Count);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            var entries = req.Alarms
                .Select(a => new ClockAlarmEntry(
                    Repeat: a.Repeat,
                    Monday: a.Monday,
                    Tuesday: a.Tuesday,
                    Wednesday: a.Wednesday,
                    Thursday: a.Thursday,
                    Friday: a.Friday,
                    Saturday: a.Saturday,
                    Sunday: a.Sunday,
                    Hour: a.Hour,
                    Minute: a.Minute,
                    Title: a.Title))
                .ToList();

            var cmd = new ClockAlarmCommand(DeviceId: deviceId, Alarms: entries);
            result = await _commandClient.SetAlarmsAsync(cmd, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetClockAlarms — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetClockAlarms — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    /// <summary>Clear all clock alarms on a device.</summary>
    [HttpDelete("clock-alarms")]
    public async Task<IActionResult> ClearClockAlarmsAsync(string deviceId, CancellationToken ct)
    {
        _logger.LogInformation("ClearClockAlarms — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.ClearAlarmsAsync(deviceId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "ClearClockAlarms — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "ClearClockAlarms — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── Sedentary reminders ───────────────────────────────────────────────────

    /// <summary>Set sedentary reminders on a device (max 3 entries).</summary>
    [HttpPut("sedentary")]
    public async Task<IActionResult> SetSedentaryAsync(
        string deviceId,
        [FromBody] SetSedentaryRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "SetSedentary — entry, device: {DeviceId}, count: {Count}",
            deviceId, req.Sedentaries.Count);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            var entries = req.Sedentaries
                .Select(s => new SedentaryEntry(
                    Repeat: s.Repeat,
                    Monday: s.Monday,
                    Tuesday: s.Tuesday,
                    Wednesday: s.Wednesday,
                    Thursday: s.Thursday,
                    Friday: s.Friday,
                    Saturday: s.Saturday,
                    Sunday: s.Sunday,
                    StartHour: s.StartHour,
                    EndHour: s.EndHour,
                    Duration: s.Duration,
                    Threshold: s.Threshold))
                .ToList();

            var cmd = new SedentaryCommand(DeviceId: deviceId, Sedentaries: entries);
            result = await _commandClient.SetSedentaryAsync(cmd, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetSedentary — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetSedentary — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    /// <summary>Clear all sedentary reminders on a device.</summary>
    [HttpDelete("sedentary")]
    public async Task<IActionResult> ClearSedentaryAsync(string deviceId, CancellationToken ct)
    {
        _logger.LogInformation("ClearSedentary — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.ClearSedentaryAsync(deviceId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "ClearSedentary — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "ClearSedentary — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }
}
