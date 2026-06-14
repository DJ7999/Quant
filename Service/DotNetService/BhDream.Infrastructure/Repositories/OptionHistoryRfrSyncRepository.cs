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

                // Use a subquery to apply the LIMIT, then insert
                string sql = $@"
            INSERT INTO ""OptionHistoryRfrSync"" (
                ""OptionHistoryId"", ""Date"", ""RfrTenor"", ""RfrMarket"", 
                ""ProcessingStatus"", ""UpdatedAt"", ""StatusChangedAt""
            )
            SELECT * FROM (
                SELECT 
                    oh.""Id"", oh.""Date"", rfr.""Tenor"", rfr.""Market"", 
                    0, NOW(), NOW()
                FROM ""OptionHistories"" oh
                INNER JOIN ""RiskFreeRates"" rfr ON oh.""Date""::date = rfr.""Date""::date
                LEFT JOIN ""OptionHistoryRfrSync"" sync ON 
                    oh.""Id"" = sync.""OptionHistoryId"" AND 
                    rfr.""Market"" = sync.""RfrMarket"" AND 
                    rfr.""Tenor"" = sync.""RfrTenor""
                WHERE sync.""OptionHistoryId"" IS NULL
                LIMIT {batchSize}
            ) AS batch
            ON CONFLICT (""OptionHistoryId"", ""RfrTenor"", ""RfrMarket"") 
            DO NOTHING;";

                do
                {
                    rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(sql);

                    // Log progress for monitoring
                    if (rowsAffected > 0)
                    {
                        Console.WriteLine($"Inserted a batch of {rowsAffected} records.");
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
