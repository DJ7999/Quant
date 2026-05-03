using BhDream.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Services.Contracts
{
    public interface IOptionsAnalyticsService
    {
        public Task<List<string>> GetUnderlyings(string pattern = "");
        public Task<List<string>> GetOptionContract(OptionContractDto contractFilter);
        Task<List<OptionHistoryWithRiskMetricsDto>> GetOptionHistoryWithRiskMetrics(OptionContractDto filter);
    }
}
