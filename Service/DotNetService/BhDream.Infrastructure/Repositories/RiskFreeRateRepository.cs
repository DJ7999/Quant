using BhDream.Application.Abstractions.Repositories;
using BhDream.Domain.Entities;
using BhDream.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Infrastructure.Repositories
{
    public class RiskFreeRateRepository : IRiskFreeRateRepository
    {
        private readonly QuantDbContext _context;

        public RiskFreeRateRepository(QuantDbContext context)
        {
            _context = context;
        }

        public async Task<List<RiskFreeRate>> GetRiskFreeRateInRange(DateTime startDate, DateTime endDate)
        {
            return await _context.RiskFreeRate
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .OrderBy(r => r.Date)
                .ToListAsync();
        }

        public async Task<List<string>> GetDistinctTenor()
        {
            return await _context.RiskFreeRate.Select(r => r.Tenor).Distinct().ToListAsync();
        }

        public async Task InsertAsync(List<RiskFreeRate> riskFreeRates)
        {
            if (riskFreeRates == null)
                throw new ArgumentNullException(nameof(riskFreeRates));

            if (riskFreeRates.Count == 0)
                return;

            // AddRangeAsync is efficient for bulk inserts; SaveChanges persisted here for convenience.
            await _context.RiskFreeRate.AddRangeAsync(riskFreeRates);
        }
    }
}
