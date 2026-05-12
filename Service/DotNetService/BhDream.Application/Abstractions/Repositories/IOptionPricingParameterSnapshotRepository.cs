using BhDream.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Abstractions.Repositories
{
    public interface IOptionPricingParameterSnapshotRepository
    {
        public Task<List<OptionPricingParameterSnapshot>> GetAnyNOptionPricingParameterSnapshotsAsync(int count);
    }
}
