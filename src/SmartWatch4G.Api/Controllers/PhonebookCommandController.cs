using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Interfaces.Services;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// App-facing endpoints to manage the phonebook on a device via the iwown entservice.
///
/// Routes:
///   PUT    /api/v1/devices/{deviceId}/phonebook — sync phonebook (max 8 entries, ≥ 1 SOS)
///   DELETE /api/v1/devices/{deviceId}/phonebook — clear all phonebook entries
/// </summary>
[ApiVersion("1.0")]
[EnableRateLimiting("dashboard-api")]
[ApiController]
[Route("api/v{version:apiVersion}/devices/{deviceId}/phonebook")]
public sealed class PhonebookCommandController : ControllerBase
{
    private readonly IWownCommandClient _commandClient;
    private readonly ILogger<PhonebookCommandController> _logger;

    public PhonebookCommandController(
        IWownCommandClient commandClient,
        ILogger<PhonebookCommandController> logger)
    {
        _commandClient = commandClient;
        _logger = logger;
    }

    /// <summary>
    /// Sync a phonebook to a device (max 8 entries).
    /// At least one entry should be marked as SOS.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> SyncPhonebookAsync(
        string deviceId,
        [FromBody] SyncPhonebookRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "SyncPhonebook — entry, device: {DeviceId}, entries: {Count}",
            deviceId, req.PhoneBook.Count);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            var entries = req.PhoneBook
                .Select(e => new PhonebookEntry(e.Name, e.Number, e.IsSos))
                .ToList();

            var cmd = new PhonebookCommand(
                DeviceId: deviceId,
                PhoneBook: entries,
                Forbid: req.Forbid);

            result = await _commandClient.SyncPhonebookAsync(cmd, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SyncPhonebook — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "SyncPhonebook — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }

    /// <summary>Clear all phonebook entries on a device.</summary>
    [HttpDelete]
    public async Task<IActionResult> ClearPhonebookAsync(string deviceId, CancellationToken ct)
    {
        _logger.LogInformation("ClearPhonebook — entry, device: {DeviceId}", deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new CommandResultDto { ReturnCode = 400 });

        CommandResult result;
        try
        {
            result = await _commandClient.ClearPhonebookAsync(deviceId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "ClearPhonebook — command failed for device {DeviceId}", deviceId);
            return StatusCode(500, new CommandResultDto { ReturnCode = 500 });
        }

        _logger.LogInformation(
            "ClearPhonebook — exit, device: {DeviceId}, returnCode: {ReturnCode}",
            deviceId, result.ReturnCode);

        return Ok(new CommandResultDto { ReturnCode = result.ReturnCode, Success = result.Success });
    }
}
