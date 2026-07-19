using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BhDream.Application.Hypothesis.DTO;
using BhDream.Application.Services.Contracts;
using Microsoft.Extensions.Configuration;

namespace BhDream.Application.Services
{
    public class HypothesisTestingService : IHypothesisTestingService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _pythonServiceUrl;

        public HypothesisTestingService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            // Retrieve Python service URL from Configuration, or default to http://localhost:8000
            _pythonServiceUrl = configuration["ExternalServices:PythonServiceUrl"] ?? "http://localhost:8000";
        }

        public Task<StrategyMetadataDto> GetHypothesisMetadataAsync()
        {
            var metadata = new StrategyMetadataDto
            {
                // Defined options strategy blueprints for UI dynamic configuration rendering
                Strategies = new List<StrategyTemplateDto>
                {
                    new StrategyTemplateDto
                    {
                        StrategyName = "BullCallSpread",
                        DisplayName = "Bull Call Spread",
                        Description = "Long ATM Option Call + Short Out-of-the-money Option Call.",
                        DefaultLegs = new List<LegTemplateDto>
                        {
                            new LegTemplateDto { LegType = "Option", IsLong = true, IsCall = false, MoneynessOffset = 0.0, ExpiryTenorDays = 30 },
                            new LegTemplateDto { LegType = "Option", IsLong = false, IsCall = false, MoneynessOffset = 0.05, ExpiryTenorDays = 30 }
                        }
                    },
                    new StrategyTemplateDto
                    {
                        StrategyName = "BearCallSpread",
                        DisplayName = "Bear Call Spread",
                        Description = "Short ATM Option Call + Long Out-of-the-money Option Call.",
                        DefaultLegs = new List<LegTemplateDto>
                        {
                            new LegTemplateDto { LegType = "Option", IsLong = false, IsCall = false, MoneynessOffset = 0.0, ExpiryTenorDays = 30 },
                            new LegTemplateDto { LegType = "Option", IsLong = true, IsCall = false, MoneynessOffset = 0.05, ExpiryTenorDays = 30 }
                        }
                    },
                    new StrategyTemplateDto
                    {
                        StrategyName = "BullPutSpread",
                        DisplayName = "Bull Put Spread",
                        Description = "Long Out-of-the-money Option Put + Short ATM Option Put.",
                        DefaultLegs = new List<LegTemplateDto>
                        {
                            new LegTemplateDto { LegType = "Option", IsLong = true, IsCall = true, MoneynessOffset = -0.05, ExpiryTenorDays = 30 },
                            new LegTemplateDto { LegType = "Option", IsLong = false, IsCall = true, MoneynessOffset = 0.0, ExpiryTenorDays = 30 }
                        }
                    },
                    new StrategyTemplateDto
                    {
                        StrategyName = "BearPutSpread",
                        DisplayName = "Bear Put Spread",
                        Description = "Long ATM Option Put + Short Out-of-the-money Option Put.",
                        DefaultLegs = new List<LegTemplateDto>
                        {
                            new LegTemplateDto { LegType = "Option", IsLong = true, IsCall = true, MoneynessOffset = 0.0, ExpiryTenorDays = 30 },
                            new LegTemplateDto { LegType = "Option", IsLong = false, IsCall = true, MoneynessOffset = -0.05, ExpiryTenorDays = 30 }
                        }
                    },
                    new StrategyTemplateDto
                    {
                        StrategyName = "LongStraddle",
                        DisplayName = "Long Straddle",
                        Description = "Long ATM Option Call + Long ATM Option Put (Volatility play).",
                        DefaultLegs = new List<LegTemplateDto>
                        {
                            new LegTemplateDto { LegType = "Option", IsLong = true, IsCall = false, MoneynessOffset = 0.0, ExpiryTenorDays = 30 },
                            new LegTemplateDto { LegType = "Option", IsLong = true, IsCall = true, MoneynessOffset = 0.0, ExpiryTenorDays = 30 }
                        }
                    },
                    new StrategyTemplateDto
                    {
                        StrategyName = "LongStrangle",
                        DisplayName = "Long Strangle",
                        Description = "Long OTM Option Call + Long OTM Option Put.",
                        DefaultLegs = new List<LegTemplateDto>
                        {
                            new LegTemplateDto { LegType = "Option", IsLong = true, IsCall = false, MoneynessOffset = 0.03, ExpiryTenorDays = 30 },
                            new LegTemplateDto { LegType = "Option", IsLong = true, IsCall = true, MoneynessOffset = -0.03, ExpiryTenorDays = 30 }
                        }
                    },
                    new StrategyTemplateDto
                    {
                        StrategyName = "IronCondor",
                        DisplayName = "Iron Condor",
                        Description = "Short OTM Put Spread + Short OTM Call Spread (Range-bound play).",
                        DefaultLegs = new List<LegTemplateDto>
                        {
                            new LegTemplateDto { LegType = "Option", IsLong = true, IsCall = true, MoneynessOffset = -0.05, ExpiryTenorDays = 30 },
                            new LegTemplateDto { LegType = "Option", IsLong = false, IsCall = true, MoneynessOffset = -0.025, ExpiryTenorDays = 30 },
                            new LegTemplateDto { LegType = "Option", IsLong = false, IsCall = false, MoneynessOffset = 0.025, ExpiryTenorDays = 30 },
                            new LegTemplateDto { LegType = "Option", IsLong = true, IsCall = false, MoneynessOffset = 0.05, ExpiryTenorDays = 30 }
                        }
                    },
                    new StrategyTemplateDto
                    {
                        StrategyName = "CustomStrategy",
                        DisplayName = "Custom Multi-Leg Strategy",
                        Description = "Add custom option legs.",
                        DefaultLegs = new List<LegTemplateDto>()
                    }
                },

                // Schema rules for option legs parameters configuration
                LegParameters = new List<LegParameterMetadataDto>
                {
                    new LegParameterMetadataDto
                    {
                        Key = "legType",
                        Label = "Asset/Leg Type",
                        InputType = "select",
                        DefaultValue = "Option",
                        Options = new List<string> { "Option" }
                    },
                    new LegParameterMetadataDto
                    {
                        Key = "isLong",
                        Label = "Direction",
                        InputType = "select",
                        DefaultValue = "Long",
                        Options = new List<string> { "Long", "Short" }
                    },
                    new LegParameterMetadataDto
                    {
                        Key = "isCall",
                        Label = "Option Type",
                        InputType = "select",
                        DefaultValue = "Call",
                        Options = new List<string> { "Call", "Put" },
                        VisibleIfProperty = "legType",
                        VisibleIfValues = new List<object> { "Option" }
                    },
                    new LegParameterMetadataDto
                    {
                        Key = "moneynessOffset",
                        Label = "Moneyness Offset",
                        InputType = "number",
                        DefaultValue = 0.0,
                        VisibleIfProperty = "legType",
                        VisibleIfValues = new List<object> { "Option" }
                    },
                    new LegParameterMetadataDto
                    {
                        Key = "expiryTenorDays",
                        Label = "Expiry (Days)",
                        InputType = "number",
                        DefaultValue = 30,
                        VisibleIfProperty = "legType",
                        VisibleIfValues = new List<object> { "Option" }
                    }
                }
            };

            return Task.FromResult(metadata);
        }

        public async Task<string> RunBacktestAsync(HypothesisBacktestRequestDto request)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_pythonServiceUrl}/api/v1/hypothesis";

            var jsonPayload = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return jsonResponse;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to run hypothesis backtest on Python service: {ex.Message}", ex);
            }
        }
    }
}
