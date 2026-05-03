using BhDream.Application.Dtos;
using BhDream.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Helpers
{
    public interface IRfrCsvParser
    {
        public Task<List<RiskFreeRate>> ParseAsync(Stream stream);
    }
}
