using System;
using System.Threading.Tasks;
using BhDream.Application.Hypothesis.DTO;
using BhDream.Application.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace BhDream.WebAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class HypothesisController : ControllerBase
    {
        private readonly IHypothesisTestingService _hypothesisService;

        public HypothesisController(IHypothesisTestingService hypothesisService)
        {
            _hypothesisService = hypothesisService;
        }

        [HttpGet("metadata")]
        public async Task<IActionResult> GetMetadata()
        {
            var result = await _hypothesisService.GetHypothesisMetadataAsync();
            return Ok(result);
        }

        [HttpPost("backtest")]
        public async Task<IActionResult> RunBacktest([FromBody] HypothesisBacktestRequestDto request)
        {
            try
            {
                var result = await _hypothesisService.RunBacktestAsync(request);
                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
