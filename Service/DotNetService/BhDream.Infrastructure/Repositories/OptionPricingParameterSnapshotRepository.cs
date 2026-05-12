using BhDream.Application.Abstractions.Repositories;
using BhDream.Domain.Entities;
using BhDream.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BhDream.Infrastructure.Repositories
{
    public class OptionPricingParameterSnapshotRepository : IOptionPricingParameterSnapshotRepository
    {
        private readonly QuantDbContext _dbContext;
        private readonly ILogger<OptionPricingParameterSnapshotRepository> _logger;
        public OptionPricingParameterSnapshotRepository(ILogger<OptionPricingParameterSnapshotRepository> logger, QuantDbContext quantDbContext) 
        { 
            _dbContext = quantDbContext;
            _logger = logger;
        }
        /// <summary>
        /// Fetches a batch of snapshots from the view. 
        /// Because of our updated View logic, this will only return rows that are 
        /// Pending (0), Stuck (1 > 10m), or Stale (Updated > StatusChanged).
        /// </summary>
        public async Task<List<OptionPricingParameterSnapshot>> GetAnyNOptionPricingParameterSnapshotsAsync(int count)
        {
            try
            {
                return await _dbContext.OptionPricingParameterSnapshots
                    .AsNoTracking() // Improves performance for read-only queue operations
                    .Take(count)    // This translates to the LIMIT clause we discussed
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching {Count} snapshots from the database view.", count);
                return new List<OptionPricingParameterSnapshot>();
            }
        }
    }
}
