using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Interfaces.Services;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// App-facing endpoints to configure device settings via the iwown entservice.
///
/// Routes (all PUT under /api/v1/devices/{deviceId}/settings/):
///   PUT  fall-detection           — enable / disable fall detection
///   PUT  fall-detection/sensitivity — set sensitivity threshold
///   PUT  wrist-gesture            — wrist-raise wake-screen window
///   PUT  data-frequency           — GPS auto-check and power mode
///   PUT  locate-frequency         — data-upload and auto-locate intervals
///   PUT  hr-interval              — heart-rate measurement interval
///   PUT  other-interval           — non-HR measurement interval
///   PUT  gps-locate               — GPS locate settings
///   PUT  time-format              — 12 / 24-hour display
///   PUT  date-format              — MM/DD vs DD/MM
///   PUT  distance-unit            — metric vs imperial
///   PUT  temperature-unit         — Celsius vs Fahrenheit
///   PUT  wear-hand                — left vs right wrist
///   PUT  bp-calibration           — blood-pressure calibration reference
///   PUT  bp-schedule              — blood-pressure measurement schedule
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("app-read")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/settings")]
public sealed class DeviceSettingsCommandController : ControllerBase
{
    private readonly IWownCommandClient _commandClient;
    private readonly ILogger<DeviceSettingsCommandController> _logger;

    public DeviceSettingsCommandController(
        IWownCommandClient commandClient,
        ILogger<DeviceSettingsCommandController> logger)
    {
        _commandClient = commandClient;
        _logger = logger;
    }

    // ── Fall detection ────────────────────────────────────────────────────────

    /// <summary>Enable or disable fall detection on a device.</summary>
    [HttpPut("fall-detection")]
    public async Task<IActionResult> SetFallDetectionAsync(
        string deviceId,
        [FromQuery] bool enabled,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "SetFallDetection — entry, device: {DeviceId}, enabled: {Enabled}", deviceId, enabled);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.SetFallDetectionAsync(deviceId, enabled, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetFallDetection — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetFallDetection — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    /// <summary>Set fall-detection sensitivity threshold (default: 14000).</summary>
    [HttpPut("fall-detection/sensitivity")]
    public async Task<IActionResult> SetFallSensitivityAsync(
        string deviceId,
        [FromBody] SetFallSensitivityRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "SetFallSensitivity — entry, device: {DeviceId}, threshold: {Threshold}",
            deviceId, req.FallThreshold);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.SetFallSensitivityAsync(deviceId, req.FallThreshold, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetFallSensitivity — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetFallSensitivity — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── Wrist gesture ─────────────────────────────────────────────────────────

    /// <summary>Configure the wrist-raise wake-screen window.</summary>
    [HttpPut("wrist-gesture")]
    public async Task<IActionResult> SetWristGestureAsync(
        string deviceId,
        [FromBody] SetWristGestureRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation("SetWristGesture — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            var cmd = new WristGestureCommand(
                DeviceId: deviceId,
                Open: req.Open,
                StartHour: req.StartHour,
                EndHour: req.EndHour);

            result = await _commandClient.SetWristGestureAsync(cmd, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetWristGesture — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetWristGesture — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── Data frequency ────────────────────────────────────────────────────────

    /// <summary>Set GPS auto-check interval and power mode.</summary>
    [HttpPut("data-frequency")]
    public async Task<IActionResult> SetDataFrequencyAsync(
        string deviceId,
        [FromBody] SetDataFrequencyRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation("SetDataFrequency — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            var cmd = new DataFrequencyCommand(
                DeviceId: deviceId,
                GpsAutoCheck: req.GpsAutoCheck,
                GpsIntervalMinutes: req.GpsIntervalMinutes,
                PowerMode: req.PowerMode);

            result = await _commandClient.SetDataFrequencyAsync(cmd, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetDataFrequency — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetDataFrequency — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    /// <summary>Set data-upload and auto-locate intervals independently.</summary>
    [HttpPut("locate-frequency")]
    public async Task<IActionResult> SetLocateDataUploadFrequencyAsync(
        string deviceId,
        [FromBody] SetLocateDataUploadRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation("SetLocateFrequency — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            var cmd = new LocateDataUploadCommand(
                DeviceId: deviceId,
                DataAutoUpload: req.DataAutoUpload,
                DataUploadIntervalMinutes: req.DataUploadIntervalMinutes,
                AutoLocate: req.AutoLocate,
                LocateIntervalMinutes: req.LocateIntervalMinutes,
                PowerMode: req.PowerMode);

            result = await _commandClient.SetLocateDataUploadFrequencyAsync(cmd, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetLocateFrequency — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetLocateFrequency — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── Measurement intervals ─────────────────────────────────────────────────

    /// <summary>Set heart-rate measurement interval (minimum 1 minute).</summary>
    [HttpPut("hr-interval")]
    public async Task<IActionResult> SetHrMeasureIntervalAsync(
        string deviceId,
        [FromBody] SetHrMeasureIntervalRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "SetHrInterval — entry, device: {DeviceId}, interval: {Interval}",
            deviceId, req.IntervalMinutes);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.SetHrMeasureIntervalAsync(
                deviceId, req.IntervalMinutes, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetHrInterval — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetHrInterval — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    /// <summary>Set non-HR health measurement interval (SpO2, BP, stress — minimum 5 minutes).</summary>
    [HttpPut("other-interval")]
    public async Task<IActionResult> SetOtherMeasureIntervalAsync(
        string deviceId,
        [FromBody] SetOtherMeasureIntervalRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "SetOtherInterval — entry, device: {DeviceId}, interval: {Interval}",
            deviceId, req.IntervalMinutes);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.SetOtherMeasureIntervalAsync(
                deviceId, req.IntervalMinutes, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetOtherInterval — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetOtherInterval — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── GPS ───────────────────────────────────────────────────────────────────

    /// <summary>Configure GPS locate settings.</summary>
    [HttpPut("gps-locate")]
    public async Task<IActionResult> SetGpsLocateAsync(
        string deviceId,
        [FromBody] SetGpsLocateRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation("SetGpsLocate — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            var cmd = new GpsLocateCommand(
                DeviceId: deviceId,
                GpsAutoCheck: req.GpsAutoCheck,
                GpsIntervalMinutes: req.GpsIntervalMinutes,
                RunGps: req.RunGps);

            result = await _commandClient.SetGpsLocateAsync(cmd, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetGpsLocate — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetGpsLocate — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── Display format / unit settings ────────────────────────────────────────

    /// <summary>Set time display format (12-hour or 24-hour).</summary>
    [HttpPut("time-format")]
    public async Task<IActionResult> SetTimeFormatAsync(
        string deviceId,
        [FromBody] SetTimeFormatRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "SetTimeFormat — entry, device: {DeviceId}, 12h: {Use12h}", deviceId, req.Use12Hour);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.SetTimeFormatAsync(deviceId, req.Use12Hour, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetTimeFormat — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetTimeFormat — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    /// <summary>Set date display format (MM/DD or DD/MM).</summary>
    [HttpPut("date-format")]
    public async Task<IActionResult> SetDateFormatAsync(
        string deviceId,
        [FromBody] SetDateFormatRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "SetDateFormat — entry, device: {DeviceId}, dayFirst: {DayFirst}",
            deviceId, req.DayFirst);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.SetDateFormatAsync(deviceId, req.DayFirst, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetDateFormat — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetDateFormat — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    /// <summary>Set distance unit (metric or imperial).</summary>
    [HttpPut("distance-unit")]
    public async Task<IActionResult> SetDistanceUnitAsync(
        string deviceId,
        [FromBody] SetDistanceUnitRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "SetDistanceUnit — entry, device: {DeviceId}, imperial: {Imperial}",
            deviceId, req.Imperial);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.SetDistanceUnitAsync(deviceId, req.Imperial, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetDistanceUnit — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetDistanceUnit — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    /// <summary>Set temperature unit (Celsius or Fahrenheit).</summary>
    [HttpPut("temperature-unit")]
    public async Task<IActionResult> SetTemperatureUnitAsync(
        string deviceId,
        [FromBody] SetTemperatureUnitRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "SetTemperatureUnit — entry, device: {DeviceId}, fahrenheit: {Fahrenheit}",
            deviceId, req.Fahrenheit);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.SetTemperatureUnitAsync(deviceId, req.Fahrenheit, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetTemperatureUnit — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetTemperatureUnit — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    /// <summary>Set which wrist the device is worn on.</summary>
    [HttpPut("wear-hand")]
    public async Task<IActionResult> SetWearHandAsync(
        string deviceId,
        [FromBody] SetWearHandRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "SetWearHand — entry, device: {DeviceId}, rightHand: {RightHand}",
            deviceId, req.RightHand);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.SetWearHandAsync(deviceId, req.RightHand, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetWearHand — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetWearHand — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── Blood pressure ────────────────────────────────────────────────────────

    /// <summary>Set blood-pressure calibration reference values.</summary>
    [HttpPut("bp-calibration")]
    public async Task<IActionResult> SetBpCalibrationAsync(
        string deviceId,
        [FromBody] SetBpCalibrationRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation("SetBpCalibration — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            var cmd = new BpCalibrationCommand(
                DeviceId: deviceId,
                SbpBand: req.SbpBand,
                DbpBand: req.DbpBand,
                SbpMeter: req.SbpMeter,
                DbpMeter: req.DbpMeter);

            result = await _commandClient.SetBpCalibrationAsync(cmd, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetBpCalibration — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetBpCalibration — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    /// <summary>Set a blood-pressure measurement schedule (up to 48 time-points, e.g. "08:00").</summary>
    [HttpPut("bp-schedule")]
    public async Task<IActionResult> SetBpScheduleAsync(
        string deviceId,
        [FromBody] SetBpScheduleRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "SetBpSchedule — entry, device: {DeviceId}, count: {Count}",
            deviceId, req.MeasureTimes.Count);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.SetBpMeasureScheduleAsync(deviceId, req.MeasureTimes, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SetBpSchedule — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetBpSchedule — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }
}
