using BhDream.Application.Dtos;
using BhDream.Application.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace BhDream.WebAPI.Controller
{
    [ApiController]
    [Route("api/option-history")]
    public class OptionHistoryController : ControllerBase
    {
        private readonly IOptionCsvImportService _importService;
        private readonly IOptionsAnalyticsService _optionsAnalyticsService;
        public OptionHistoryController(IOptionCsvImportService importService, IOptionsAnalyticsService optionsAnalyticsService)
        {
            _importService = importService;
            _optionsAnalyticsService = optionsAnalyticsService;
        }

        [HttpPost("upload-csv")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var stream = file.OpenReadStream();

            var result = await _importService.ImportAsync(stream);

            return Ok(result);
        }

        [HttpGet("get-underlyings")]
        public async Task<IActionResult> GetUnderlyings()
        {
            var result = await _optionsAnalyticsService.GetUnderlyings();
            return Ok(result);
        }

        [HttpPost("get-option-contract")]
        public async Task<IActionResult> GetOptionContract([FromBody]OptionContractDto filter)
        {
            var result = await _optionsAnalyticsService.GetOptionContract(filter);
            return Ok(result);
        }

        [HttpPost("get-option-History")]
        public async Task<IActionResult> GetOptionHistory([FromBody] OptionContractDto filter)
        {
            var result = await _optionsAnalyticsService.GetOptionHistoryWithRiskMetrics(filter);
            return Ok(result);
        }
    }
}
