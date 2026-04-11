using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Interfaces.Services;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// App-facing endpoints to send core commands to a device via the iwown entservice.
///
/// Routes:
///   GET  /api/v1/devices/{deviceId}/status             — query device online/offline status
///   POST /api/v1/devices/{deviceId}/commands/userinfo  — push user profile
///   POST /api/v1/devices/{deviceId}/commands/datasync  — trigger one-shot data sync
///   POST /api/v1/devices/{deviceId}/commands/location  — request real-time GPS fix
///   POST /api/v1/devices/{deviceId}/commands/factory-reset — factory reset
///   POST /api/v1/devices/{deviceId}/commands/message   — push notification message
///   POST /api/v1/devices/{deviceId}/commands/language  — set display language
///   POST /api/v1/devices/{deviceId}/commands/goal      — set activity goals
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("dashboard-api")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}")]
public sealed class DeviceCommandsController : ControllerBase
{
    private readonly IWownCommandClient _commandClient;
    private readonly ILogger<DeviceCommandsController> _logger;

    public DeviceCommandsController(
        IWownCommandClient commandClient,
        ILogger<DeviceCommandsController> logger)
    {
        _commandClient = commandClient;
        _logger = logger;
    }

    // ── Status ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Query the live online/offline status of a device directly from the iwown platform.
    /// This calls the iwown entservice in real-time — distinct from the stored status-event history
    /// at GET /api/v1/devices/{deviceId}/status which reads the local database.
    /// </summary>
    [HttpGet("commands/status")]
    public async Task<IActionResult> GetDeviceStatusAsync(string deviceId, CancellationToken ct)
    {
        _logger.LogInformation("GetDeviceStatus — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new DeviceOnlineStatusDto { ReturnCode = 400 });

        DeviceOnlineStatus? status;
        try
        {
            status = await _commandClient.GetDeviceStatusAsync(deviceId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDeviceStatus — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new DeviceOnlineStatusDto { ReturnCode = 500 });
        }

        if (status is null)
        {
            _logger.LogWarning("GetDeviceStatus — no result for device {DeviceId}", deviceId);
            return NotFound(new DeviceOnlineStatusDto { ReturnCode = 404 });
        }

        _logger.LogInformation(
            "GetDeviceStatus — exit, device: {DeviceId}, statusCode: {StatusCode}",
            deviceId, status.StatusCode);

        return Ok(new DeviceOnlineStatusDto
        {
            ReturnCode = 0,
            DeviceId = status.DeviceId,
            StatusCode = status.StatusCode,
            IsOnline = status.IsOnline
        });
    }

    // ── User profile ──────────────────────────────────────────────────────────

    /// <summary>Push user profile (height, weight, age, gender) to a device.</summary>
    [HttpPost("commands/userinfo")]
    public async Task<IActionResult> SendUserInfoAsync(
        string deviceId,
        [FromBody] SendUserInfoRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation("SendUserInfo — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            var cmd = new UserInfoCommand(
                DeviceId: deviceId,
                Height: req.Height,
                Weight: req.Weight,
                Gender: req.Gender,
                Age: req.Age,
                WristCircle: req.WristCircle,
                Hypertension: req.Hypertension);

            result = await _commandClient.SendUserInfoAsync(cmd, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SendUserInfo — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SendUserInfo — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── Data sync ─────────────────────────────────────────────────────────────

    /// <summary>Trigger a one-shot data sync from a device.</summary>
    [HttpPost("commands/datasync")]
    public async Task<IActionResult> TriggerDataSyncAsync(string deviceId, CancellationToken ct)
    {
        _logger.LogInformation("TriggerDataSync — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.TriggerDataSyncAsync(deviceId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TriggerDataSync — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "TriggerDataSync — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── Real-time location ────────────────────────────────────────────────────

    /// <summary>Request an immediate real-time GPS location fix from a device.</summary>
    [HttpPost("commands/location")]
    public async Task<IActionResult> RequestRealtimeLocationAsync(string deviceId, CancellationToken ct)
    {
        _logger.LogInformation("RequestRealtimeLocation — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.RequestRealtimeLocationAsync(deviceId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "RequestRealtimeLocation — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "RequestRealtimeLocation — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── Factory reset ─────────────────────────────────────────────────────────

    /// <summary>Factory-reset a device.</summary>
    [HttpPost("commands/factory-reset")]
    public async Task<IActionResult> FactoryResetAsync(string deviceId, CancellationToken ct)
    {
        _logger.LogInformation("FactoryReset — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.FactoryResetAsync(deviceId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FactoryReset — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "FactoryReset — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── Message ───────────────────────────────────────────────────────────────

    /// <summary>Push a notification message to a device (title ≤ 15 bytes, description ≤ 240 bytes).</summary>
    [HttpPost("commands/message")]
    public async Task<IActionResult> SendMessageAsync(
        string deviceId,
        [FromBody] SendMessageRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation("SendMessage — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.SendMessageAsync(deviceId, req.Title, req.Description, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SendMessage — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SendMessage — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── Language ──────────────────────────────────────────────────────────────

    /// <summary>Set the display language on a device.</summary>
    [HttpPost("commands/language")]
    public async Task<IActionResult> SetLanguageAsync(
        string deviceId,
        [FromBody] SetLanguageRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "SetLanguage — entry, device: {DeviceId}, code: {Code}", deviceId, req.LanguageCode);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.SetLanguageAsync(deviceId, req.LanguageCode, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SetLanguage — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetLanguage — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── Goals ─────────────────────────────────────────────────────────────────

    /// <summary>Set daily step / distance / calorie goals on a device.</summary>
    [HttpPost("commands/goal")]
    public async Task<IActionResult> SetGoalAsync(
        string deviceId,
        [FromBody] SetGoalRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation("SetGoal — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.SetGoalAsync(
                deviceId, req.Step, req.DistanceMetres, req.CalorieKcal, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SetGoal — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SetGoal — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static IActionResult CommandOk(CommandResult r) =>
        new OkObjectResult(new CommandResultDto { ReturnCode = r.ReturnCode, Success = r.Success });
}
