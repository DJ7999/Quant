using BhDream.Application.Abstractions.Repositories;
using BhDream.Domain.Entities;
using BhDream.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace BhDream.Infrastructure.Repositories
{
    public class UnderlyingRepository : IUnderlyingRepository
    {
        private readonly QuantDbContext _context;

        public UnderlyingRepository(QuantDbContext context)
        {
            _context = context;
        }

        public async  Task AddUnderlying(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentNullException(nameof(symbol));
            
            await _context.Underlyings.AddAsync(new Underlying { Symbol = symbol});
        }

        public async Task<Underlying?> GetUnderlyingBySymbol(string symbol)
        {
            return await _context.Underlyings.SingleOrDefaultAsync(ul =>
            string.Equals(ul.Symbol, symbol)
            );
        }

        public async Task<List<Underlying>> GetUnderlyingsAsync(string filter = "")
        {
            var pattern = $"%{filter}%";
            return await _context.Underlyings
                .Where(u => EF.Functions.Like(u.Symbol, pattern))
                .ToListAsync();
        }
    }
}
