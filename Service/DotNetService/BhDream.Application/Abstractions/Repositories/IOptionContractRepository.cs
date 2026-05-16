using BhDream.Application.Dtos;
using BhDream.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Abstractions.Repositories
{
    public interface IOptionContractRepository
    {
        public Task<OptionContract?> GetOptionContract(OptionContract contract);
        public Task<OptionContract> AddOptionContract(OptionContract contract);
        public Task<List<OptionContract>> GetOptionContractsAsync(OptionContractDto contract);
    }
}
