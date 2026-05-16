using BhDream.Application.Dtos;
using BhDream.Application.Dtos.BhDream.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Services.Contracts
{
    public interface IOptionsAnalyticsService
    {
        public Task<List<string>> GetUnderlyings(string pattern = "");
        public Task<List<OptionContractDto>> GetOptionContract(OptionContractDto contractFilter);

        public Task<List<OptionHistoryDto>> GetOptionContractHistory(OptionContractDto contractFilter);
        public Task<List<OptionGreeksAndIvDto>> GetOptionContractHistoryGreeksAndIv(OptionContractDto contractFilter, string tenor);
        public Task<List<string>> GetRFRTenor();
    }
}
