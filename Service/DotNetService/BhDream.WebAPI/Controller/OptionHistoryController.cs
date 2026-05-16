using BhDream.Application.Dtos;
using BhDream.Application.Dtos.BhDream.Domain.Entities;
using BhDream.Application.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Diagnostics;

namespace BhDream.WebAPI.Controller
{
    [ApiController]
    [Route("api/option-history")]
    public class OptionHistoryController : ControllerBase
    {
        private readonly IOptionCsvImportService _importService;
        private readonly IOptionsAnalyticsService _optionsAnalyticsService;
        private readonly ILogger<OptionHistoryController> _logger;
        public OptionHistoryController(IOptionCsvImportService importService, IOptionsAnalyticsService optionsAnalyticsService, ILogger<OptionHistoryController> logger)
        {
            _importService = importService;
            _optionsAnalyticsService = optionsAnalyticsService;
            _logger = logger;
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
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                List<string> underlyings = await _optionsAnalyticsService.GetUnderlyings();
                return Ok(underlyings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching underlyings.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation($"GetUnderlyings executed in {stopwatch.ElapsedMilliseconds} ms.");
            }
        }


        [HttpPost("get-contract-expiries")]
        public async Task<IActionResult> GetOptionContractExpiries([FromBody] OptionContractDto filter)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                var optionContracts = await _optionsAnalyticsService.GetOptionContract(filter);
                var expiries = optionContracts.Select(c => c.ExpirationDate).Distinct().ToList();
                return Ok(expiries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching ContractExpiries.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation($"GetOptionContractExpiries executed in {stopwatch.ElapsedMilliseconds} ms.");
            }
        }

        [HttpPost("get-contract-strikes")]
        public async Task<IActionResult> GetOptionContractStrikes([FromBody] OptionContractDto filter)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                var optionContracts = await _optionsAnalyticsService.GetOptionContract(filter);
                var strikes = optionContracts.Select(c => c.StrikePrice).Distinct().ToList();
                return Ok(strikes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching ContractStrikes.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation($"GetOptionContractStrikes executed in {stopwatch.ElapsedMilliseconds} ms.");
            }
        }

        [HttpPost("get-contract-history")]
        public async Task<IActionResult> GetOptionContractHistory([FromBody] OptionContractDto filter)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                var optionHistory = await _optionsAnalyticsService.GetOptionContractHistory(filter);
               
                return Ok(optionHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching OptionContractHistory.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation($"GetOptionContractHistory executed in {stopwatch.ElapsedMilliseconds} ms.");
            }
        }

        [HttpPost("get-contract-history-greeks-iv")]
        public async Task<IActionResult> GetOptionContractHistoryGreeksAndIv([FromBody] OptionContractDto filter, [FromQuery] string tenor)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                List<OptionGreeksAndIvDto> optionHistoryGreeksAndIv = await _optionsAnalyticsService.GetOptionContractHistoryGreeksAndIv(filter, tenor);

                return Ok(optionHistoryGreeksAndIv);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching GetOptionContractHistoryGreeks.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation($"GetOptionContractHistoryGreeksAndIv executed in {stopwatch.ElapsedMilliseconds} ms.");
            }
        }

        [HttpGet("get-tenor")]
        public async Task<IActionResult> GetTenor()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                List<string> optionHistoryGreeksAndIv = await _optionsAnalyticsService.GetRFRTenor();

                return Ok(optionHistoryGreeksAndIv);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching GetTenor.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation($"GetTenor executed in {stopwatch.ElapsedMilliseconds} ms.");
            }
        }
    }
}
