using BhDream.Application.Abstractions.Repositories;
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
    public class OptionHistoryRfrSyncRepository : IOptionHistoryRfrSyncRepository
    {
        private readonly QuantDbContext _dbContext;
        private readonly ILogger<OptionHistoryRfrSync> _logger;
        public OptionHistoryRfrSyncRepository(ILogger<OptionHistoryRfrSync> logger, QuantDbContext quantDbContext)
        {
            _dbContext = quantDbContext;
            _logger = logger;
        }
        public async Task UpdateStatus(List<OptionHistoryRfrSync> optionHistoryRfrSyncs, ProcessingStatus status)
        {
            if (optionHistoryRfrSyncs == null || !optionHistoryRfrSyncs.Any())
                return;

            try
            {
                var now = DateTime.Now;

                // 1. Prepare keys in memory as UPPERCASE
                var localKeys = optionHistoryRfrSyncs
                    .Select(s => $"{s.OptionHistoryId}_{s.RfrMarket}_{s.RfrTenor}".ToUpperInvariant())
                    .ToHashSet();

                // 2. Force the DB side to UPPERCASE during the match
                // Note: db.OptionHistoryId.ToString() might need ToUpper() if it's a string in DB
                var rowsAffected = await _dbContext.OptionHistoryRfrSync
                    .Where(db => localKeys.Contains(
                        db.OptionHistoryId.ToString().ToUpper() + "_" +
                        db.RfrMarket.ToUpper() + "_" +
                        db.RfrTenor.ToUpper()))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.ProcessingStatus, status)
                        .SetProperty(s => s.StatusChangedAt, now)
                    );

                _logger.LogInformation("Bulk updated {Count} sync records to status {Status}. {}", optionHistoryRfrSyncs.Count, status, rowsAffected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed bulk update.");
                throw;
            }
        }
    }
}
