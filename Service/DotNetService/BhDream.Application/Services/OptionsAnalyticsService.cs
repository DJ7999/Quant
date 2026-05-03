using BhDream.Application.Dtos;
using BhDream.Application.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Services
{
    public class OptionsAnalyticsService : IOptionsAnalyticsService
    {
        public Task<List<string>> GetOptionContract(OptionContractDto contractFilter)
        {
            throw new NotImplementedException();
        }

        public Task<List<OptionHistoryWithRiskMetricsDto>> GetOptionHistoryWithRiskMetrics(OptionContractDto filter)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetUnderlyings(string pattern = "")
        {
            throw new NotImplementedException();
        }
    }
}
