using BhDream.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Abstractions.Repositories
{
    public interface IRiskFreeRateRepository
    {
        Task InsertAsync(List<RiskFreeRate> riskFreeRates);
        Task<List<RiskFreeRate>> GetRiskFreeRateInRange(DateTime startDate, DateTime endDate);
        Task<List<string>> GetDistinctTenor();
    }
}
