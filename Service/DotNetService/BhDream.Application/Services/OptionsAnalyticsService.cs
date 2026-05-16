using BhDream.Application.Abstractions.Repositories;
using BhDream.Application.Dtos;
using BhDream.Application.Dtos.BhDream.Domain.Entities;
using BhDream.Application.Services.Contracts;
using BhDream.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Services
{
    public class OptionsAnalyticsService : IOptionsAnalyticsService
    {
        private readonly ILogger<OptionsAnalyticsService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public OptionsAnalyticsService(ILogger<OptionsAnalyticsService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<OptionContractDto>> GetOptionContract(OptionContractDto contractFilter)
        {
            var optionContractModels = await _unitOfWork.OptionContractRepository.GetOptionContractsAsync(contractFilter);
            return optionContractModels.Select(OptionContractDto.FromEntity).ToList();
        }

        public async Task<List<OptionHistoryDto>> GetOptionContractHistory(OptionContractDto contractFilter)
        {
            var optionHistoryModels = await _unitOfWork.OptionHistoryRepository.GetOptionHistoryForContractAsync(contractFilter);
            return optionHistoryModels.Select(OptionHistoryDto.FromEntity).ToList();
        }

        public async Task<List<OptionGreeksAndIvDto>> GetOptionContractHistoryGreeksAndIv(OptionContractDto contractFilter, string tenor)
        {
            var greekIvModels = await _unitOfWork.OptionGreeksAndIvRepository.GetOptionGreeksAndIvForContractAsync(contractFilter,tenor);
            return greekIvModels.Select(OptionGreeksAndIvDto.FromEntity).ToList();
        }

        public async Task<List<string>> GetRFRTenor()
        {
            return await _unitOfWork.RiskFreeRateRepository.GetDistinctTenor();
        }

        public async Task<List<string>> GetUnderlyings(string pattern = "")
        {
            var underlyings = await _unitOfWork.UnderlyingRepository.GetUnderlyingsAsync(pattern);
            return underlyings.Select(ul=>ul.Symbol).ToList();
        }
    }
}
