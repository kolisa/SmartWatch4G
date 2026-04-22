using Microsoft.AspNetCore.Mvc;
using SampleApi.Iwown;
using SampleApi.Services;

namespace SampleApi.Controllers
{
    [ApiController]
    [Route("calculation")]
    public class IwownCalculationController : ControllerBase
    {
        private readonly IwownCalculationService _calc;

        public IwownCalculationController(IwownCalculationService calc)
        {
            _calc = calc;
        }

        [HttpPost("sleep")]
        public async Task<IActionResult> CalculateSleep([FromBody] SleepCalculationRequest req) =>
            Ok(await _calc.CalculateSleepAsync(req));

        [HttpPost("ecg")]
        public async Task<IActionResult> CalculateEcg([FromBody] EcgCalculationRequest req) =>
            Ok(await _calc.CalculateEcgAsync(req));

        [HttpPost("af")]
        public async Task<IActionResult> CalculateAf([FromBody] AfCalculationRequest req) =>
            Ok(await _calc.CalculateAfAsync(req));

        [HttpPost("spo2")]
        public async Task<IActionResult> CalculateSpo2([FromBody] Spo2CalculationRequest req) =>
            Ok(await _calc.CalculateSpo2Async(req));
    }
}
