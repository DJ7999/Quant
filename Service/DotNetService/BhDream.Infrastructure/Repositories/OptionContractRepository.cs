using BhDream.Application.Abstractions.Repositories;
using BhDream.Application.Dtos;
using BhDream.Domain.Entities;
using BhDream.Domain.Enums;
using BhDream.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BhDream.Infrastructure.Repositories
{
    public class OptionContractRepository : IOptionContractRepository
    {
        private readonly QuantDbContext _context;

        public OptionContractRepository(QuantDbContext context)
        {
            _context = context;
        }

        public async Task<OptionContract> AddOptionContract(OptionContract contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));
            // ensure required FK is present
            if (contract.UnderlyingId == Guid.Empty)
                throw new ArgumentException("UnderlyingId is required");

            await _context.OptionContracts.AddAsync(contract);
           
            return contract;
        }

        public async Task<OptionContract?> GetOptionContract(OptionContract contract)
        {
            return await _context.OptionContracts.SingleOrDefaultAsync(oc =>
                oc.StrikePrice == contract.StrikePrice &&
                oc.OptionType == contract.OptionType &&
                oc.Expiry == contract.Expiry &&
                oc.UnderlyingId == contract.UnderlyingId);
        }

        public async Task<List<OptionContract>> GetOptionContractsAsync(OptionContractDto contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            var query = _context.OptionContracts
                .Include(oc => oc.Underlying)
                .AsQueryable();

            if (!string.IsNullOrEmpty(contract.Underlying))
            {
                // SQL translation supports .ToUpper() or direct equality
                // Direct equality is usually case-insensitive in most SQL setups (like SQLite/SQL Server default)
                query = query.Where(oc => oc.Underlying.Symbol.ToUpper() == contract.Underlying.ToUpper());
            }

            if (Enum.TryParse<OptionRightType>(contract.OptionType, ignoreCase: true, out var parsedType))
            {
                query = query.Where(oc => oc.OptionType == parsedType);
            }

            if (contract.StrikePrice.HasValue)
            {
                query = query.Where(oc => oc.StrikePrice == contract.StrikePrice.Value);
            }

            if (contract.ExpirationDate.HasValue)
            {
                // Use the Date property directly if supported, or stick to your range check
                // 1. Get the local IST date at midnight
                var localDate = contract.ExpirationDate.Value.Date;

                // 2. Define your IST boundaries for that entire day
                var istStart = DateTime.SpecifyKind(localDate, DateTimeKind.Unspecified);
                var istEnd = istStart.AddDays(1);

                // 3. Find your IST zone info (you might want to expose this as a static helper from your DbContext)
                var utcStart = TimeZoneInfo.ConvertTimeToUtc(istStart, QuantDbContext.IstZone);
                var utcEnd = TimeZoneInfo.ConvertTimeToUtc(istEnd, QuantDbContext.IstZone);
                // 5. Query using the UTC boundaries
                query = query.Where(oc => oc.Expiry >= utcStart && oc.Expiry < utcEnd);
            }

            return await query.ToListAsync();
        }
    }
}
