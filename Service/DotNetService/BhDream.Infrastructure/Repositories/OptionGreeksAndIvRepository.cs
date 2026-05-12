using BhDream.Application.Abstractions.Repositories;
using BhDream.Domain.Entities;
using BhDream.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Infrastructure.Repositories
{
    public class OptionGreeksAndIvRepository : IOptionGreeksAndIvRepository
    {
        private readonly QuantDbContext _context;
        private readonly ILogger<OptionGreeksAndIvRepository> _logger;

        public OptionGreeksAndIvRepository(QuantDbContext context, ILogger<OptionGreeksAndIvRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddUpdateOptionGreeksAndIvBatch(List<OptionGreeksAndIv> results)
        {
            if (results == null || !results.Any()) return;
            List<OptionGreeksAndIv> newRecords = new List<OptionGreeksAndIv>();
            // 1. Fetch potential existing records based on the main indexed IDs
            var historyIds = results.Select(r => r.OptionHistoryId).Distinct().ToList();
            var contractIds = results.Select(r => r.ContractId).Distinct().ToList();

            var existingRecords = await _context.OptionGreeksAndIvs
                .Where(db => historyIds.Contains(db.OptionHistoryId) && contractIds.Contains(db.ContractId))
                .ToListAsync();

            // 2. Create a lookup map for the composite key
            var existingMap = existingRecords.ToDictionary(
                e => $"{e.ContractId}_{e.OptionHistoryId}_{e.RfrMarket}_{e.RfrTenor}");

            foreach (var incoming in results)
            {
                var key = $"{incoming.ContractId}_{incoming.OptionHistoryId}_{incoming.RfrMarket}_{incoming.RfrTenor}";

                if (existingMap.TryGetValue(key, out var existing))
                {
                    // 3. Update the existing tracked entity
                    existing.Delta = incoming.Delta;
                    existing.Gamma = incoming.Gamma;
                    existing.Theta = incoming.Theta;
                    existing.Vega = incoming.Vega;
                    existing.Rho = incoming.Rho;
                    existing.Vomma = incoming.Vomma;
                    existing.ImpliedVolatility = incoming.ImpliedVolatility;

                    existing.BenchMarkDelta = incoming.BenchMarkDelta;
                    existing.BenchMarkGamma = incoming.BenchMarkGamma;
                    existing.BenchMarkTheta = incoming.BenchMarkTheta;
                    existing.BenchMarkVega = incoming.BenchMarkVega;
                    existing.BenchMarkRho = incoming.BenchMarkRho;
                    existing.BenchMarkVomma = incoming.BenchMarkVomma;
                    existing.BenchMarkImpliedVolatility = incoming.BenchMarkImpliedVolatility;

                    existing.CalculatedAt = incoming.CalculatedAt;
                }
                else
                {
                    // 4. Add as a new record
                    newRecords.Add(incoming);
                }
            }
            await _context.OptionGreeksAndIvs.AddRangeAsync(newRecords);
        }
    }
}
