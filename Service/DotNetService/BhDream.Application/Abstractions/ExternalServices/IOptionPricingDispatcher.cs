using BhDream.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Abstractions.ExternalServices
{
    public interface IOptionPricingDispatcher
    {
        /// <summary>
        /// Sends a batch of option data to the high-performance C++ pricing engine.
        /// </summary>
        Task Dispatch(IEnumerable<OptionPricingParameterSnapshot> requestSnapshots, Guid batchId);
    }
}
