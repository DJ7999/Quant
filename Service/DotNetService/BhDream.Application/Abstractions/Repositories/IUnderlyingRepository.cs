using BhDream.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Abstractions.Repositories
{
    public interface IUnderlyingRepository
    {
        public Task<Underlying?> GetUnderlyingBySymbol(string symbol);
        public Task AddUnderlying(string symbol);

        public Task<List<Underlying>> GetUnderlyingsAsync(string filter = "");
    }
}
