using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Abstractions.Repositories
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        public IUnderlyingRepository UnderlyingRepository { get; set; }
        public IOptionContractRepository OptionContractRepository { get; set; }
        public IOptionHistoryRepository OptionHistoryRepository { get; set; }
        public IRiskFreeRateRepository RiskFreeRateRepository { get; set; }
        public IOptionPricingParameterSnapshotRepository OptionPricingParameterSnapshotRepository { get; set; }
        public IOptionHistoryRfrSyncRepository OptionHistoryRfrSyncRepository { get; set; }
        public IOptionGreeksAndIvRepository OptionGreeksAndIvRepository { get; set; }
    }
}
