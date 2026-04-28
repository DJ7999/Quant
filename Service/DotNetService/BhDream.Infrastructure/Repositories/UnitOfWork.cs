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

        public UnitOfWork(
            QuantDbContext context,
            IOptionHistoryRepository optionHistoryRepository,
            IUnderlyingRepository underlyingRepository,
            IOptionContractRepository optionContractRepository)
        {
            _context = context;
            OptionHistoryRepository = optionHistoryRepository;
            UnderlyingRepository = underlyingRepository;
            OptionContractRepository = optionContractRepository;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
