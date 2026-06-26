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

        public async Task<List<OptionHistory>> GetOptionHistoryForContractAsync(OptionContractDto entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // 1. Parse the Enum safely beforehand
            if (!Enum.TryParse<OptionRightType>(entity.OptionType, ignoreCase: true, out var parsedType))
            {
                throw new ArgumentException("Invalid Option Type provided.", nameof(entity));
            }

            // 2. Handle the Date boundaries using your centralized IST Zone
            if (!entity.ExpirationDate.HasValue)
            {
                throw new ArgumentException("Expiration Date is required.", nameof(entity));
            }

            var localDate = entity.ExpirationDate.Value.Date;
            var istStart = DateTime.SpecifyKind(localDate, DateTimeKind.Unspecified);

            // Convert local midnight to UTC database equivalent
            var utcStart = TimeZoneInfo.ConvertTimeToUtc(istStart, QuantDbContext.IstZone);

            // 3. Optimize the Symbol matching (assuming DB symbols are upper-case)
            var upperSymbol = entity.Underlying?.ToUpper();

            // 4. Run the clean, index-friendly query
            return await _context.OptionHistories
                .Include(h => h.Contract)
                .Where(h => h.Contract.Underlying.Symbol == upperSymbol
                            && h.Contract.Expiry == utcStart
                            && h.Contract.StrikePrice == entity.StrikePrice
                            && h.Contract.OptionType == parsedType)
                .ToListAsync();
        }

        public async Task<(DateTime,DateTime)> GetFirstAndLastDate()
        {
            DateTime minDateAvailable = await _context.OptionHistories.MinAsync(h => h.Date);
            DateTime maxDateAvailable = await _context.OptionHistories.MaxAsync(h => h.Date);
            return (minDateAvailable, maxDateAvailable);
        }
    }
}
