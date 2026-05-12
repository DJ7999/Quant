using BhDream.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Abstractions.Repositories
{
    public interface IOptionGreeksAndIvRepository
    {
        /// <summary>
        /// Adds a batch of option greeks and IV results to the database.
        /// </summary>
        Task AddUpdateOptionGreeksAndIvBatch(List<OptionGreeksAndIv> results);
    }
}
