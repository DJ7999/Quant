using BhDream.Application.Abstractions.Repositories;
using BhDream.Application.Dtos;
using BhDream.Application.Helpers;
using BhDream.Application.Mapper;
using BhDream.Application.Services.Contracts;
using BhDream.Domain.Entities;
using BhDream.Domain.Enums;
using System.Diagnostics.Contracts;

namespace BhDream.Application.Services
{
    public class OptionCsvImportService : IOptionCsvImportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptionHistoryCsvParser _parser;

        private const int HistorySaveBatchSize = 1000;
        public OptionCsvImportService(IUnitOfWork unitOfWork, IOptionHistoryCsvParser parser) {
            _parser = parser;
            _unitOfWork = unitOfWork;
        }

        public async Task<OptionCsvImportResult> ImportAsync(Stream csvStream)
        {
            var parsedRows = await _parser.ParseAsync(csvStream);
            var mapped = parsedRows.Select(OptionHistoryMapper.Map).ToList();

            // 1. dedupe identical CSV input rows early
            var uniqueRows = DeduplicateCsvRows(mapped);

            // 2. preload/create all underlyings used in the file (one round-trip per distinct symbol)
            var underlyingCache = await PreloadUnderlyingsAsync(uniqueRows);
            Console.WriteLine("underlying done");

            // 3. preload/create option contracts (one round-trip per distinct contract)
            var contractCache = await PreloadOptionContractsAsync(uniqueRows, underlyingCache);
            Console.WriteLine("contractCache done");
            // 4. process histories in batches using caches
            var result = await ProcessHistoriesAsync(uniqueRows, underlyingCache, contractCache);
            Console.WriteLine("Histories done");
            return result;
        }

        private List<OptionHistory> DeduplicateCsvRows(List<OptionHistory> rows)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var unique = new List<OptionHistory>(rows.Count);

            foreach (var r in rows)
            {
                var key = $"{r.Contract.Underlying.Symbol}|{r.Contract.Expiry:O}|{r.Contract.StrikePrice}|{(int)r.Contract.OptionType}|{r.Date:O}";
                if (seen.Add(key))
                    unique.Add(r);
                else
                    key = null; // duplicate row, skip
            }

            return unique;
        }

        private async Task<Dictionary<string, Underlying>> PreloadUnderlyingsAsync(IEnumerable<OptionHistory> rows)
        {
            var symbols = rows.Select(r => r.Contract.Underlying.Symbol)
                              .Distinct(StringComparer.OrdinalIgnoreCase)
                              .ToList();

            var cache = new Dictionary<string, Underlying>(StringComparer.OrdinalIgnoreCase);
            var toCreate = new List<string>();

            foreach (var symbol in symbols)
            {
                var existing = await _unitOfWork.UnderlyingRepository.GetUnderlyingBySymbol(symbol);
                if (existing != null)
                {
                    cache[symbol] = existing;
                }
                else
                {
                    toCreate.Add(symbol);
                    // add tracked entities via repository
                    await _unitOfWork.UnderlyingRepository.AddUnderlying(symbol);
                }
            }

            if (toCreate.Count > 0)
            {
                await _unitOfWork.SaveChangesAsync();

                // refresh created underlyings into cache
                foreach (var symbol in toCreate)
                {
                    var u = await _unitOfWork.UnderlyingRepository.GetUnderlyingBySymbol(symbol);
                    if (u != null)
                        cache[symbol] = u;
                }
            }

            return cache;
        }

        private async Task<Dictionary<string, OptionContract>> PreloadOptionContractsAsync(IEnumerable<OptionHistory> rows, Dictionary<string, Underlying> underlyingCache)
        {
            // build unique contract keys from rows
            var contractKeys = rows
                .Select(r => new
                {
                    Symbol = r.Contract.Underlying.Symbol,
                    Expiry = r.Contract.Expiry,
                    Strike = r.Contract.StrikePrice,
                    Type = r.Contract.OptionType
                })
                .Distinct()
                .ToList();

            var cache = new Dictionary<string, OptionContract>(StringComparer.OrdinalIgnoreCase);
            var contractsToCreate = new List<OptionContract>();

            foreach (var k in contractKeys)
            {
                var underlying = underlyingCache[k.Symbol];
                var lookup = new OptionContract
                {
                    UnderlyingId = underlying.Id,
                    Expiry = k.Expiry,
                    StrikePrice = k.Strike,
                    OptionType = k.Type
                };

                var existing = await _unitOfWork.OptionContractRepository.GetOptionContract(lookup);
                var cacheKey = ContractCacheKey(underlying.Id, k.Expiry, k.Strike, k.Type);

                if (existing != null)
                {
                    cache[cacheKey] = existing;
                }
                else
                {
                    // create tracked entity and add to cache
                    var newContract = new OptionContract
                    {
                        UnderlyingId = underlying.Id,
                        Underlying = underlying,
                        Expiry = k.Expiry,
                        StrikePrice = k.Strike,
                        OptionType = k.Type
                    };

                    await _unitOfWork.OptionContractRepository.AddOptionContract(newContract);
                    contractsToCreate.Add(newContract);
                    cache[cacheKey] = newContract;
                }
            }

            if (contractsToCreate.Count > 0)
            {
                try
                {
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception)
                {
                    // Unique-constraint race possible; re-query any contract still not persisted
                    foreach (var kv in cache.ToList())
                    {
                        var c = kv.Value;
                        if (c.Id == Guid.Empty)
                        {
                            var reLookup = new OptionContract
                            {
                                UnderlyingId = c.UnderlyingId,
                                Expiry = c.Expiry,
                                StrikePrice = c.StrikePrice,
                                OptionType = c.OptionType
                            };

                            var fetched = await _unitOfWork.OptionContractRepository.GetOptionContract(reLookup);
                            if (fetched != null)
                                cache[kv.Key] = fetched;
                        }
                    }

                    // if any contract remains unresolved, rethrow
                    if (cache.Values.Any(c => c == null || c.Id == Guid.Empty))
                        throw;
                }
            }

            return cache;
        }

        private async Task<OptionCsvImportResult> ProcessHistoriesAsync(
            List<OptionHistory> rows,
            Dictionary<string, Underlying> underlyingCache,
            Dictionary<string, OptionContract> contractCache)
        {
            int inserted = 0, updated = 0;
            var seenHistoryByContractDate = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int processed = 0;

            foreach (var row in rows)
            {
                // resolve underlying/contract from cache
                var symbol = row.Contract.Underlying.Symbol;
                var underlying = underlyingCache[symbol];
                row.Contract.Underlying = underlying;
                row.Contract.UnderlyingId = underlying.Id;

                var contractKey = ContractCacheKey(underlying.Id, row.Contract.Expiry, row.Contract.StrikePrice, row.Contract.OptionType);
                if (!contractCache.TryGetValue(contractKey, out var contract) || contract == null)
                    throw new InvalidOperationException($"Contract not found in cache for key {contractKey}");

                row.Contract = contract;
                row.ContractId = contract.Id;

                var historyKey = $"{row.ContractId:N}|{row.Date:O}";
                if (!seenHistoryByContractDate.Add(historyKey))
                    continue; // duplicate history in same file

                var addList = new List<OptionHistory>();
                var existing = await _unitOfWork.OptionHistoryRepository.GetOptionHistoryAsync(row);
                if (existing == null)
                {
                    addList.Add(row);
                    
                    inserted++;
                }
                else
                {
                    // ensure we update the tracked entity correctly: set Id and call UpdateAsync
                    row.Id = existing.Id;
                    await _unitOfWork.OptionHistoryRepository.UpdateAsync(row);
                    updated++;
                }

                processed++;
                if (processed % HistorySaveBatchSize == 0)
                {
                    await _unitOfWork.OptionHistoryRepository.AddRangeAsync(addList);
                    await _unitOfWork.SaveChangesAsync();
                    Console.WriteLine($"remaining = {rows.Count-processed}");
                }
            }

            // final save
            await _unitOfWork.SaveChangesAsync();

            return new OptionCsvImportResult
            {
                InsertedCount = inserted,
                UpdatedCount = updated
            };
        }

        private static string ContractCacheKey(Guid underlyingId, DateTime expiry, decimal strike, OptionRightType type)
            => $"{underlyingId:N}|{expiry:O}|{strike}|{(int)type}";
    }
}
