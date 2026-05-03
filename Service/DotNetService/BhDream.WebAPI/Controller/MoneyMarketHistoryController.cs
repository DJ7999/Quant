using BhDream.Application.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace BhDream.WebAPI.Controller
{
    [ApiController]
    [Route("api/money-market-history")]
    public class MoneyMarketHistoryController : ControllerBase
    {
        private readonly IRfrCsvImportService _rfrimportService;
        public MoneyMarketHistoryController(IRfrCsvImportService rfrimportService)
        {
            _rfrimportService = rfrimportService;
        }

        [HttpPost("upload-csv")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var stream = file.OpenReadStream();

            await _rfrimportService.ImportAsync(stream);

            return Ok();
        }
    }
}
