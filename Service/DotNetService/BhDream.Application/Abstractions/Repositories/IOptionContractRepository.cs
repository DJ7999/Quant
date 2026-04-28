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
    }
}
