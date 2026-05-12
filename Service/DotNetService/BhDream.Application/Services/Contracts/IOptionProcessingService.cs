using BhDream.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Services.Contracts
{
    public interface IOptionProcessingService
    {
        Task<List<OptionPricingParameterSnapshot>> GetUnProcessedOptionParameters(int limit = 1000);
        Task SaveOptionGreeksResults(List<OptionGreeksAndIv> results);
    }
}
