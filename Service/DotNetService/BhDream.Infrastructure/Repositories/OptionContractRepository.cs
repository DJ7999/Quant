using BhDream.Application.Abstractions.Repositories;
using BhDream.Domain.Entities;
using BhDream.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
