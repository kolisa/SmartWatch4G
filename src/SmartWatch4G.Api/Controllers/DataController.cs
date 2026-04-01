using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SmartWatch4G.Domain.Interfaces.Services;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Receives binary history-data packets from wearable devices (opcode 0x80)
/// and OldMan (OM0) location packets (opcode 0x0A).
/// Route: POST /pb/upload
/// </summary>
[EnableRateLimiting("device-write")]
[Route("pb/upload")]
[ApiController]
public sealed class DataController : PacketParserBase
{
    private readonly IProtobufPacketHandler _handler;
    private readonly ILogger<DataController> _logger;

    protected override ILogger Logger => _logger;

    public DataController(
        IProtobufPacketHandler handler,
        ILogger<DataController> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    [HttpPost]
    public Task<IActionResult> UploadPbDataAsync(CancellationToken ct)
    {
        _logger.LogInformation("UploadPbData — entry from {RemoteIp}",
            HttpContext.Connection.RemoteIpAddress);
        return ParseAndDispatchAsync(ct);
    }

    protected override Task OnPacketAsync(
        string deviceId,
        ushort opcode,
        byte[] payload,
        CancellationToken ct)
        => _handler.HandleAsync(deviceId, opcode, payload, ct);
}
