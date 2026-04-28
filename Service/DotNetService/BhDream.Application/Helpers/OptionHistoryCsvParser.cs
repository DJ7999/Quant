using BhDream.Application.Dtos;
using BhDream.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Helpers
{
    public class OptionHistoryCsvParser : IOptionHistoryCsvParser
    {
        public async Task<List<OptionHistoryCsvRow>> ParseAsync(Stream stream)
        {
            using var reader = new StreamReader(stream);

            var list = new List<OptionHistoryCsvRow>();

            await reader.ReadLineAsync(); // header

            string? line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                var cols = line.Split(',');

                if (cols.Length < 17)
                    continue;

                try
                {
                    var row = new OptionHistoryCsvRow
                    {
                        Symbol = cols[0].Trim(),
                        Date = DateTime.Parse(cols[1]),
                        Expiry = DateTime.Parse(cols[2]),
                        OptionType = CommonHelper.ParseOptionType(cols[3]).Value,
                        StrikePrice = decimal.Parse(cols[4]),

                        Open = ParseDecimal(cols[5]),
                        High = ParseDecimal(cols[6]),
                        Low = ParseDecimal(cols[7]),
                        Close = ParseDecimal(cols[8]),
                        Ltp = ParseDecimal(cols[9]),
                        SettlePrice = ParseDecimal(cols[10]),

                        NumberOfContracts = ParseLong(cols[11]),
                        Turnover = ParseDecimal(cols[12]),
                        PremiumTurnover = ParseDecimal(cols[13]),
                        OpenInterest = ParseLong(cols[14]),

                        ChangeInUnderlyingValue = ParseDecimal(cols[15]),
                        UnderlyingValue = ParseDecimal(cols[16])
                    };

                    list.Add(row);
                }
                catch
                {
                    continue; // skip bad row
                }
            }

            return list;
        }

        private decimal? ParseDecimal(string s)
            => decimal.TryParse(s, out var v) ? v : null;

        private long? ParseLong(string s)
            => long.TryParse(s, out var v) ? v : null;

        private double? ParseDouble(string s)
            => double.TryParse(s, out var v) ? v : null;
    }
}
