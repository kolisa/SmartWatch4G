using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Interfaces;
using SmartWatch4G.Infrastructure.Services;

namespace KhoiWatchData.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/calculation")]
public class IwownCalculationController : ControllerBase
{
    private readonly IwownCalculationService _calc;
    private readonly IDatabaseService _db;

    public IwownCalculationController(IwownCalculationService calc, IDatabaseService db)
    {
        _calc = calc;
        _db   = db;
    }

    [HttpPost("sleep")]
    public async Task<IActionResult> CalculateSleep([FromBody] SleepCalculationRequest req)
    {
        var result = await _calc.CalculateSleepAsync(req);
        if (result is null) return StatusCode(502);
        if (result.ReturnCode == 0 && result.Success != false)
            await _db.InsertSleepCalculation(
                req.device_id,
                req.recordDate,
                result.completed,
                result.start_time,
                result.end_time,
                result.hr,
                result.turn_times,
                result.respiratory?.avg,
                result.respiratory?.max,
                result.respiratory?.min,
                result.sections is not null
                    ? JsonSerializer.Serialize(result.sections)
                    : null);
        return Ok(result);
    }

    [HttpPost("ecg")]
    public async Task<IActionResult> CalculateEcg([FromBody] EcgCalculationRequest req)
    {
        var result = await _calc.CalculateEcgAsync(req);
        if (result is null) return StatusCode(502);
        if (result.ReturnCode == 0 && result.Success != false)
            await _db.InsertEcgCalculation(req.device_id, result.result, result.hr, result.effective, result.direction);
        return Ok(result);
    }

    [HttpPost("af")]
    public async Task<IActionResult> CalculateAf([FromBody] AfCalculationRequest req)
    {
        var result = await _calc.CalculateAfAsync(req);
        if (result is null) return StatusCode(502);
        if (result.ReturnCode == 0 && result.Success != false)
            await _db.InsertAfCalculation(req.device_id, result.result);
        return Ok(result);
    }

    [HttpPost("spo2")]
    public async Task<IActionResult> CalculateSpo2([FromBody] Spo2CalculationRequest req)
    {
        var result = await _calc.CalculateSpo2Async(req);
        if (result is null) return StatusCode(502);
        if (result.ReturnCode == 0 && result.Success != false)
            await _db.InsertSpo2Calculation(req.device_id, result.spo2_score, result.osahs_risk);
        return Ok(result);
    }
}
