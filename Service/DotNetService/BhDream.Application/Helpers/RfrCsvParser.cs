using BhDream.Application.Dtos;
using BhDream.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Helpers
{
    public class RfrCsvParser : IRfrCsvParser
    {
        public async Task<List<RiskFreeRate>> ParseAsync(Stream stream)
        {
            using StreamReader reader = new StreamReader(stream);
            var list = new List<RiskFreeRate>();
            await reader.ReadLineAsync(); // header
            await reader.ReadLineAsync(); // header
            await reader.ReadLineAsync(); // header
            
            string? line;
            while((line = await reader.ReadLineAsync()) != null)
            {
                var cols = line.Split(',');
                if (cols.Length < 3)
                    continue;
                try
                {
                    var row = new RiskFreeRate
                    {
                        Date = DateTime.Parse(cols[0]),
                        Tenor = cols[2].Trim(),
                        Rate = decimal.Parse(cols[3])
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
    }
}
