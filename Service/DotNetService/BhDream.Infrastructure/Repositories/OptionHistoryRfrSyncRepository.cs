using BhDream.Application.Abstractions.Repositories;
using BhDream.Domain.Entities;
using BhDream.Domain.Enums;
using BhDream.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BhDream.Infrastructure.Repositories
{
    public class OptionHistoryRfrSyncRepository : IOptionHistoryRfrSyncRepository
    {
        private readonly QuantDbContext _dbContext;
        private readonly ILogger<OptionHistoryRfrSync> _logger;
        private static readonly SemaphoreSlim _dbLock = new SemaphoreSlim(1, 1);
        public OptionHistoryRfrSyncRepository(ILogger<OptionHistoryRfrSync> logger, QuantDbContext quantDbContext)
        {
            _dbContext = quantDbContext;
            _logger = logger;
        }
        public async Task UpdateStatus(List<OptionHistoryRfrSync> optionHistoryRfrSyncs, ProcessingStatus status)
        {
            if (optionHistoryRfrSyncs == null || !optionHistoryRfrSyncs.Any()) return;

            await _dbLock.WaitAsync();
            try
            {
                // Ensure these arrays are of the correct type
                var ids = optionHistoryRfrSyncs.Select(x => x.OptionHistoryId).ToArray();
                var markets = optionHistoryRfrSyncs.Select(x => x.RfrMarket).ToArray();
                var tenors = optionHistoryRfrSyncs.Select(x => x.RfrTenor).ToArray();

                // Changed 'int[]' to 'uuid[]' below
                string sql = @"
            UPDATE ""OptionHistoryRfrSync"" sync
            SET ""ProcessingStatus"" = @p0, 
                ""StatusChangedAt"" = @p1
            FROM (SELECT UNNEST(@p2::uuid[]) as id, 
                         UNNEST(@p3::text[]) as market, 
                         UNNEST(@p4::text[]) as tenor) as temp
            WHERE sync.""OptionHistoryId"" = temp.id 
              AND sync.""RfrMarket"" = temp.market 
              AND sync.""RfrTenor"" = temp.tenor";

                await _dbContext.Database.ExecuteSqlRawAsync(sql,
                    (int)status,
                    DateTime.UtcNow,
                    ids,
                    markets,
                    tenors
                );
            }
            finally
            {
                _dbLock.Release();
            }
        }

        public async Task UpdateSyncTableAsync()
        {
            await _dbLock.WaitAsync();
            try
            {
                int rowsAffected;
                int batchSize = 50000;

                // Clean query processing directly from our specialized unquoted view
                string sql = $@"
    INSERT INTO ""OptionHistoryRfrSync"" (
        ""OptionHistoryId"", ""Date"", ""RfrTenor"", ""RfrMarket"", 
        ""ProcessingStatus"", ""UpdatedAt"", ""StatusChangedAt""
    )
    SELECT 
        v.""OptionHistoryId"", v.""Date"", v.""RfrTenor"", v.""RfrMarket"", 
        0, NOW(), NOW()
    FROM v_OptionHistoryClosestRfr v
    WHERE NOT EXISTS (
        SELECT 1 
        FROM ""OptionHistoryRfrSync"" sync 
        WHERE v.""OptionHistoryId"" = sync.""OptionHistoryId"" 
          AND v.""RfrMarket"" = sync.""RfrMarket"" 
          AND v.""RfrTenor"" = sync.""RfrTenor""
    )
    ORDER BY v.""OptionHistoryId"", v.""RfrMarket""
    LIMIT {batchSize}
    ON CONFLICT (""OptionHistoryId"", ""RfrTenor"", ""RfrMarket"") 
    DO NOTHING;";

                do
                {
                    rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(sql);

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine($"[Sync] Processed and paired a batch of {rowsAffected} closest-tenor records.");
                    }

                } while (rowsAffected > 0);
            }
            finally
            {
                _dbLock.Release();
            }
        }
    }
}
