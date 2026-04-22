using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.Interfaces;

namespace KhoiWatchData.Api.Controllers;

[ApiController]
[Route("workers")]
public sealed class WorkerController : ControllerBase
{
    private readonly IWorkerQueryService _workerService;

    public WorkerController(IWorkerQueryService workerService)
    {
        _workerService = workerService;
    }

    /// <summary>
    /// Returns a paginated list of workers with their latest health snapshot and GPS coordinates.
    /// Supports the Worker Stats table view (Slide 4).
    /// </summary>
    /// <param name="page">Page number, 1-based (default: 1).</param>
    /// <param name="pageSize">Items per page, max 100 (default: 10).</param>
    [HttpGet]
    public async Task<IActionResult> GetWorkers(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _workerService.GetPagedWorkersAsync(page, pageSize);
        if (result.IsFailure)
            return StatusCode(500, new { message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns the full detail for a single worker: profile, latest health metrics,
    /// latest GPS position, and current device online status.
    /// Supports the Individual Worker Detail view (Slide 5).
    /// </summary>
    /// <param name="deviceId">The device ID linked to the worker.</param>
    [HttpGet("{deviceId}")]
    public async Task<IActionResult> GetWorkerDetail(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new { message = "Device ID is required." });

        var result = await _workerService.GetWorkerDetailAsync(deviceId);
        if (result.IsFailure)
            return result.ErrorCode switch
            {
                404 => NotFound(new { message = result.Error }),
                _   => StatusCode(500, new { message = result.Error })
            };

        return Ok(result.Value);
    }
}
