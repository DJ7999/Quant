using BhDream.Application.Abstractions.Repositories;
using BhDream.Application.Dtos;
using BhDream.Domain.Entities;
using BhDream.Domain.Enums;
using BhDream.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;

namespace BhDream.Infrastructure.Repositories
{
    public class OptionHistoryRepository : IOptionHistoryRepository
    {
        private readonly QuantDbContext _context;

        public OptionHistoryRepository(QuantDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(OptionHistory entity)
        {
            if (entity == null)
                return;

            await _context.OptionHistories.AddAsync(entity);
        }

        public async Task UpdateAsync(OptionHistory entity)
        {
            if (entity == null)
                return;

            // Try to find an already-tracked or existing entity to avoid tracking conflicts.
            OptionHistory? existing = null;

            // Prefer lookup by PK if available (fast)
            if (entity.Id != Guid.Empty)
            {
                existing = await _context.OptionHistories.FindAsync(entity.Id);
            }

            // Fallback to logical key (date + contract) — mirrors GetOptionHistoryAsync
            if (existing == null)
            {
                existing = await _context.OptionHistories
                    .FirstOrDefaultAsync(e => e.Date == entity.Date && e.ContractId == entity.ContractId);
            }

            
            
            // Merge values from the detached entity into the tracked entity.
            // This preserves the tracked instance and avoids "already being tracked" errors.
            _context.Entry(existing).CurrentValues.SetValues(entity);

            await Task.CompletedTask;
        }

        public async Task<OptionHistory?> GetOptionHistoryAsync(OptionHistory entity)
        {
            if (entity == null)
                return null;
            // 1) check tracked (local) entries first to catch rows added earlier in this import session
            var local = _context.OptionHistories.Local
                .FirstOrDefault(e => e.ContractId == entity.ContractId && e.Date == entity.Date);
            if (local != null)
                return local;

            // 2) fallback to database query

            // Use FirstOrDefaultAsync to avoid exceptions if duplicates exist;
            // logical uniqueness should be enforced by the DB index.
            return await _context.OptionHistories.FirstOrDefaultAsync(e =>
                e.Date == entity.Date &&
                e.ContractId == entity.ContractId
            );
        }

        public async Task AddRangeAsync(List<OptionHistory> entities)
        {
            if (entities == null || !entities.Any())
                return;

            await _context.OptionHistories.AddRangeAsync(entities);
        }

        // Define this at the class level
        private static readonly SemaphoreSlim _dbLock = new SemaphoreSlim(1, 1);

        public async Task UpsertRangeAsync(List<OptionHistory> entities)
        {
            if (entities == null || !entities.Any()) return;

            // Wait to enter the database-critical section
            await _dbLock.WaitAsync();
            try
            {
                var bulkConfig = new BulkConfig
                {
                    // These properties identify which records to update
                    UpdateByProperties = new List<string> { nameof(OptionHistory.ContractId), nameof(OptionHistory.Date) },

                    // Explicitly exclude the Primary Key from the update set 
                    // to prevent triggering the Foreign Key violation (23503)
                    PropertiesToExcludeOnUpdate = new List<string> { nameof(OptionHistory.Id) },

                    // Performance optimizations
                    SetOutputIdentity = false,
                    BatchSize = 1000
                };

                await _context.BulkInsertOrUpdateAsync(entities, bulkConfig);
            }
            finally
            {
                // Always release the lock, even if the DB operation fails
                _dbLock.Release();
            }
        }

        public Task UpdateRangeAsync(List<OptionHistory> entity)
        {
            throw new NotImplementedException();
        }

        public async Task<List<OptionHistory>> GetOptionHistoryForContractAsync(OptionContractDto entity)
        {
             return await _context.OptionHistories
                .Include(h => h.Contract)
                .Where(h => h.Contract.Underlying.Symbol.ToUpper() == entity.Underlying.ToUpper()
                            && h.Contract.Expiry.Date == entity.ExpirationDate.Value.Date
                            && h.Contract.StrikePrice == entity.StrikePrice
                            && h.Contract.OptionType == Enum.Parse<OptionRightType>(entity.OptionType,true))
                .ToListAsync();
        }
    }
}
