using SmartWatch4G.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Receives binary alarm packets from wearable devices (opcode 0x12).
/// Route: POST /alarm/upload
/// </summary>
[Route("alarm/upload")]
[ApiController]
public sealed class AlarmController : PacketParserBase
{
    private readonly IProtobufPacketHandler _handler;
    private readonly ILogger<AlarmController> _logger;

    protected override ILogger Logger => _logger;

    public AlarmController(
        IProtobufPacketHandler handler,
        ILogger<AlarmController> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    [HttpPost]
    public Task<IActionResult> UploadAlarmDataAsync(CancellationToken ct)
        => ParseAndDispatchAsync(ct);

    protected override Task OnPacketAsync(
        string deviceId,
        ushort opcode,
        byte[] payload,
        CancellationToken ct)
        => _handler.HandleAsync(deviceId, opcode, payload, ct);
}
