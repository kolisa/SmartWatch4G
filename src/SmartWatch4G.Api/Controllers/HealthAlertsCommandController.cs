using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Interfaces.Services;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// App-facing endpoints to configure health-monitoring alerts on a device
/// via the iwown entservice.
///
/// Routes (all PUT under /api/v1/devices/{deviceId}/alerts/):
///   PUT hr               — static heart-rate alarm
///   PUT hr/dynamic       — dynamic heart-rate alarm
///   PUT spo2             — blood-oxygen alarm
///   PUT bp               — blood-pressure alarm
///   PUT temperature      — body-temperature alarm
///   PUT blood-sugar      — blood-sugar alarm
///   PUT blood-potassium  — blood-potassium alarm
///   PUT autoaf           — auto atrial-fibrillation monitoring
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/alerts")]
public sealed class HealthAlertsCommandController : ControllerBase
{
    private readonly IWownCommandClient _commandClient;
    private readonly ILogger<HealthAlertsCommandController> _logger;

    public HealthAlertsCommandController(
        IWownCommandClient commandClient,
        ILogger<HealthAlertsCommandController> logger)
    {
        _commandClient = commandClient;
        _logger = logger;
    }

    // ── Heart-rate alarm ──────────────────────────────────────────────────────

    /// <summary>Configure static heart-rate alarm thresholds.</summary>
    [HttpPut("hr")]
    public async Task<IActionResult> SetHrAlarmAsync(
        string deviceId,
        [FromBody] SetHrAlarmRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation("SetHrAlarm — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            var cmd = new HrAlarmCommand(
                DeviceId: deviceId,
                Open: req.Open,
                High: req.High,
                Low: req.Low,
                Threshold: req.Threshold,
                AlarmIntervalMinutes: req.AlarmIntervalMinutes);

            result = await _commandClient.SetHrAlarmAsync(cmd, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SetHrAlarm — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetHrAlarm — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    /// <summary>Configure dynamic heart-rate alarm thresholds.</summary>
    [HttpPut("hr/dynamic")]
    public async Task<IActionResult> SetDynamicHrAlarmAsync(
        string deviceId,
        [FromBody] SetDynamicHrAlarmRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation("SetDynamicHrAlarm — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            var cmd = new DynamicHrAlarmCommand(
                DeviceId: deviceId,
                Open: req.Open,
                High: req.High,
                Low: req.Low,
                TimeoutSeconds: req.TimeoutSeconds,
                IntervalMinutes: req.IntervalMinutes);

            result = await _commandClient.SetDynamicHrAlarmAsync(cmd, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetDynamicHrAlarm — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetDynamicHrAlarm — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── SpO2 alarm ────────────────────────────────────────────────────────────

    /// <summary>Configure blood-oxygen (SpO2) alarm threshold.</summary>
    [HttpPut("spo2")]
    public async Task<IActionResult> SetSpo2AlarmAsync(
        string deviceId,
        [FromBody] SetSpo2AlarmRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation("SetSpo2Alarm — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.SetSpo2AlarmAsync(deviceId, req.Open, req.LowThreshold, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SetSpo2Alarm — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetSpo2Alarm — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── BP alarm ──────────────────────────────────────────────────────────────

    /// <summary>Configure blood-pressure alarm thresholds.</summary>
    [HttpPut("bp")]
    public async Task<IActionResult> SetBpAlarmAsync(
        string deviceId,
        [FromBody] SetBpAlarmRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation("SetBpAlarm — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            var cmd = new BpAlarmCommand(
                DeviceId: deviceId,
                Open: req.Open,
                SbpHigh: req.SbpHigh,
                SbpBelow: req.SbpBelow,
                DbpHigh: req.DbpHigh,
                DbpBelow: req.DbpBelow);

            result = await _commandClient.SetBpAlarmAsync(cmd, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SetBpAlarm — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetBpAlarm — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── Temperature alarm ─────────────────────────────────────────────────────

    /// <summary>Configure body-temperature alarm thresholds (value × 10, e.g. 375 = 37.5 °C).</summary>
    [HttpPut("temperature")]
    public async Task<IActionResult> SetTemperatureAlarmAsync(
        string deviceId,
        [FromBody] SetTemperatureAlarmRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation("SetTemperatureAlarm — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.SetTemperatureAlarmAsync(
                deviceId, req.Open, req.High, req.Low, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetTemperatureAlarm — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetTemperatureAlarm — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── Blood-sugar alarm ─────────────────────────────────────────────────────

    /// <summary>Configure blood-sugar alarm thresholds.</summary>
    [HttpPut("blood-sugar")]
    public async Task<IActionResult> SetBloodSugarAlarmAsync(
        string deviceId,
        [FromBody] SetBloodSugarAlarmRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation("SetBloodSugarAlarm — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.SetBloodSugarAlarmAsync(
                deviceId, req.Open, req.Low, req.High, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetBloodSugarAlarm — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetBloodSugarAlarm — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── Blood-potassium alarm ─────────────────────────────────────────────────

    /// <summary>Configure blood-potassium alarm thresholds.</summary>
    [HttpPut("blood-potassium")]
    public async Task<IActionResult> SetBloodPotassiumAlarmAsync(
        string deviceId,
        [FromBody] SetBloodPotassiumAlarmRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation("SetBloodPotassiumAlarm — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.SetBloodPotassiumAlarmAsync(
                deviceId, req.Open, req.Low, req.High, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetBloodPotassiumAlarm — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetBloodPotassiumAlarm — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── Auto-AF ───────────────────────────────────────────────────────────────

    /// <summary>Configure auto atrial-fibrillation (AF) monitoring settings.</summary>
    [HttpPut("autoaf")]
    public async Task<IActionResult> SetAutoAfAsync(
        string deviceId,
        [FromBody] SetAutoAfRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation("SetAutoAf — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            var cmd = new AutoAfCommand(
                DeviceId: deviceId,
                Open: req.Open,
                IntervalSeconds: req.IntervalSeconds,
                RriSingleTime: req.RriSingleTime,
                RriType: req.RriType);

            result = await _commandClient.SetAutoAfAsync(cmd, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SetAutoAf — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetAutoAf — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }
}
