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
                var date = contract.ExpirationDate.Value.Date;
                query = query.Where(oc => oc.Expiry >= date && oc.Expiry < date.AddDays(1));
            }

            return await query.ToListAsync();
        }
    }
}
