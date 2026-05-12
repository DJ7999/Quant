using BhDream.Application.Abstractions.Repositories;
using BhDream.Application.Services.Contracts;
using BhDream.Domain.Entities;
using BhDream.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BhDream.Application.Services
{
    public class OptionProcessingService : IOptionProcessingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OptionProcessingService> _logger;
        
        public OptionProcessingService(IUnitOfWork unitOfWork, ILogger<OptionProcessingService> logger) { 
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<List<OptionPricingParameterSnapshot>> GetUnProcessedOptionParameters(int limit = 1000)
        {
            var unProcessedOptionSnapshot = await _unitOfWork.OptionPricingParameterSnapshotRepository.GetAnyNOptionPricingParameterSnapshotsAsync(limit);

            await _unitOfWork.OptionHistoryRfrSyncRepository.UpdateStatus(unProcessedOptionSnapshot.Select(x => new OptionHistoryRfrSync
            {
                OptionHistoryId = x.OptionHistoryId,
                RfrTenor = x.RfrTenor,
                RfrMarket = x.RfrMarket,
            }).ToList(), 
            ProcessingStatus.InProgress);

            await _unitOfWork.SaveChangesAsync();

            return unProcessedOptionSnapshot;
        }

        public async Task SaveOptionGreeksResults(List<OptionGreeksAndIv> results)
        {
            await _unitOfWork.OptionGreeksAndIvRepository.AddUpdateOptionGreeksAndIvBatch(results);
            await _unitOfWork.OptionHistoryRfrSyncRepository.UpdateStatus(results.Select(x => new OptionHistoryRfrSync
            {
                OptionHistoryId = x.OptionHistoryId,
                RfrTenor = x.RfrTenor,
                RfrMarket = x.RfrMarket,
            }).ToList(),ProcessingStatus.Completed);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
