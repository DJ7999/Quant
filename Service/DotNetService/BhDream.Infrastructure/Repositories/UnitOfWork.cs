using BhDream.Application.Abstractions.Repositories;
using BhDream.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BhDream.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly QuantDbContext _context;

        public IOptionHistoryRepository OptionHistoryRepository { get; set; }
        public IUnderlyingRepository UnderlyingRepository { get; set; }
        public IOptionContractRepository OptionContractRepository { get; set; }
        public IRiskFreeRateRepository RiskFreeRateRepository { get; set; }
        public IOptionPricingParameterSnapshotRepository OptionPricingParameterSnapshotRepository { get; set; }
        public IOptionHistoryRfrSyncRepository OptionHistoryRfrSyncRepository { get; set; }
        public IOptionGreeksAndIvRepository OptionGreeksAndIvRepository { get; set; }

        public UnitOfWork(
            QuantDbContext context,
            IOptionHistoryRepository optionHistoryRepository,
            IUnderlyingRepository underlyingRepository,
            IOptionContractRepository optionContractRepository,
            IRiskFreeRateRepository riskFreeRateRepository,
            IOptionPricingParameterSnapshotRepository optionPricingParameterSnapshotRepository,
            IOptionHistoryRfrSyncRepository optionHistoryRfrSyncRepository,
            IOptionGreeksAndIvRepository optionGreeksAndIvRepository)
        {
            _context = context;
            OptionHistoryRepository = optionHistoryRepository;
            UnderlyingRepository = underlyingRepository;
            OptionContractRepository = optionContractRepository;
            RiskFreeRateRepository = riskFreeRateRepository;
            OptionPricingParameterSnapshotRepository = optionPricingParameterSnapshotRepository;
            OptionHistoryRfrSyncRepository = optionHistoryRfrSyncRepository;
            OptionGreeksAndIvRepository = optionGreeksAndIvRepository;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
