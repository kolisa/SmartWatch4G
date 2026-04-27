using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces;

namespace KhoiWatchData.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/audit")]
public class AuditController : ControllerBase
{
    private readonly IDatabaseService _db;

    public AuditController(IDatabaseService db) => _db = db;

    [HttpGet("log")]
    public async Task<IActionResult> GetLog(
        [FromQuery] string? deviceId   = null,
        [FromQuery] string? action     = null,
        [FromQuery] string? tableName  = null,
        [FromQuery] System.DateTime? from  = null,
        [FromQuery] System.DateTime? to    = null,
        [FromQuery] int skip           = 0,
        [FromQuery] int take           = 50)
    {
        (IReadOnlyList<AuditEntry> items, int total) =
            await _db.GetAuditLog(deviceId, action, tableName, from, to, skip, take);
        return Ok(new { items, total });
    }
}
