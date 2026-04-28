using BhDream.Application.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace BhDream.WebAPI.Controller
{
    [ApiController]
    [Route("api/option-history")]
    public class OptionHistoryController : ControllerBase
    {
        private readonly IOptionCsvImportService _importService;
        public OptionHistoryController(IOptionCsvImportService importService)
        {
            _importService = importService;
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
    }
}
