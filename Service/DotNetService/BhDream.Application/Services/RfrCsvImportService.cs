using BhDream.Application.Abstractions.Repositories;
using BhDream.Application.Helpers;
using BhDream.Application.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Services
{
    public class RfrCsvImportService : IRfrCsvImportService
    {
        private readonly IRfrCsvParser _rfrCsvParser;
        private readonly IUnitOfWork _unitOfWork;

        public RfrCsvImportService(IRfrCsvParser rfrCsvParser, IUnitOfWork unitOfWork)
        {
            _rfrCsvParser = rfrCsvParser;
            _unitOfWork = unitOfWork;
        }

        public async Task ImportAsync(Stream csvStream)
        {
            var rfrList = await _rfrCsvParser.ParseAsync(csvStream);
            int take = 1000;
            int skip = 0;
            int remaining = rfrList.Count;
            while (remaining > 0)
            {
                var batch = rfrList.Skip(skip).Take(take).ToList();
                await _unitOfWork.RiskFreeRateRepository.InsertAsync(batch);
                await _unitOfWork.SaveChangesAsync();
                skip += take;
                remaining -= take;
            }
        }
    }
}
