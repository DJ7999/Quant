using BhDream.Application.Abstractions.Repositories;
using BhDream.Application.Helpers;
using BhDream.Application.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
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
            if (rfrList == null || rfrList.Count == 0)
                return;

            // 1. Remove any duplicates within the uploaded CSV itself in memory
            rfrList = rfrList
                .GroupBy(r => new { r.Date, r.Tenor, r.Market })
                .Select(g => g.First())
                .ToList();

            int take = 1000;
            int skip = 0;
            int remaining = rfrList.Count;

            while (remaining > 0)
            {
                var batch = rfrList.Skip(skip).Take(take).ToList();

                // 2. Perform DB checks in batches to avoid loading large date ranges at once
                var minDate = batch.Min(r => r.Date);
                var maxDate = batch.Max(r => r.Date);

                var existingRates = await _unitOfWork.RiskFreeRateRepository.GetRiskFreeRateInRange(minDate, maxDate);
                var existingKeys = new HashSet<(DateTime Date, string Tenor, string Market)>(
                    existingRates.Select(r => (r.Date, r.Tenor, r.Market))
                );

                // 3. Filter out records that are already in the database
                var newRecords = batch
                    .Where(r => !existingKeys.Contains((r.Date, r.Tenor, r.Market)))
                    .ToList();

                if (newRecords.Count > 0)
                {
                    await _unitOfWork.RiskFreeRateRepository.InsertAsync(newRecords);
                    await _unitOfWork.SaveChangesAsync();
                }

                skip += take;
                remaining -= take;
            }
        }
    }
}
