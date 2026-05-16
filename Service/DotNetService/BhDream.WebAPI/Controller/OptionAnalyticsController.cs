using BhDream.Application.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BhDream.WebAPI.Controller
{
    [ApiController]
    [Route("api/option-analytics")]
    public class OptionAnalyticsController : ControllerBase
    {
        private readonly IOptionsAnalyticsService _optionsAnalyticsService;
        private readonly ILogger<OptionAnalyticsController> _logger;
        public OptionAnalyticsController(IOptionsAnalyticsService optionsAnalyticsService, ILogger<OptionAnalyticsController> logger)
        {
            _optionsAnalyticsService = optionsAnalyticsService;
            _logger = logger;
        }
    }
}
