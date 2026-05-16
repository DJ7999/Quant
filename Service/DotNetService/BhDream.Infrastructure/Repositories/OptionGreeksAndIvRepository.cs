using BhDream.Application.Abstractions.Repositories;
using BhDream.Application.Dtos;
using BhDream.Domain.Entities;
using BhDream.Domain.Enums;
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

        public async Task<List<OptionGreeksAndIv>> GetOptionGreeksAndIvForContractAsync(OptionContractDto contractFilter, string tenor)
        {
            // 1. Prepare safe casing variables outside the database context execution
            string underlyingFilter = contractFilter.Underlying?.Trim().ToLower() ?? "";

            // 2. Safely parse the incoming string filter into your OptionType Enum wrapper
            // Replace 'OptionType' here with the actual name of your Enum if it differs (e.g., OptionTypeEnum)
            bool hasValidEnum = Enum.TryParse<OptionRightType>(contractFilter.OptionType, true, out var parsedEnum);

            return await _context.OptionGreeksAndIvs
                .Include(o => o.Contract)
                .Include(o => o.OptionHistory)
                .Where(o =>
                    // Underlying Symbol Filter
                    (underlyingFilter == "" || o.Contract.Underlying.Symbol.ToLower() == underlyingFilter) &&

                    // Enum Filter: If no option type string was provided, ignore it. 
                    // If it was provided and successfully parsed, compare directly using standard strongly typed operators.
                    (string.IsNullOrEmpty(contractFilter.OptionType) || (hasValidEnum && o.Contract.OptionType == parsedEnum)) &&

                    // Strike Price Filter
                    (!contractFilter.StrikePrice.HasValue || o.Contract.StrikePrice == contractFilter.StrikePrice.Value) &&

                    // Expiration Date Filter
                    (!contractFilter.ExpirationDate.HasValue || o.Contract.Expiry == contractFilter.ExpirationDate.Value) &&
                    (!string.IsNullOrEmpty(tenor) || o.RfrTenor.ToUpper() == tenor.ToUpper())
                )
                .ToListAsync();
        }

        
    }
}
