using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Xunit;
using BhDream.Application.Helpers;
using BhDream.Application.Dtos;
using BhDream.Domain.Enums;

namespace BhDream.Application.Tests.HelpersTests
{
    public class OptionHistoryCsvParserTests
    {
        [Fact]
        public async Task ParseAsync_ShouldParseSingleValidRow()
        {
            var header = new[]
            {
                "Symbol","Date","Expiry","OptionType","Strike",
                "Open","High","Low","Close","Ltp","Settle",
                "NumberOfContracts","Turnover","PremiumTurnover","OpenInterest",
                "ChangeInUnderlyingValue","UnderlyingValue"
            };

            var values = new[]
            {
                "ABC",
                "2023-01-02",
                "2023-02-03",
                "Call",
                "100.5",
                "1.1",
                "2.2",
                "0.9",
                "1.5",
                "1.6",
                "1.4",
                "1000",
                "50000.25",
                "250.75",
                "2000",
                "0.5",
                "150.75"
            };

            var csv = new StringBuilder();
            csv.AppendLine(string.Join(",", header));
            csv.AppendLine(string.Join(",", values));

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv.ToString()));
            var parser = new OptionHistoryCsvParser();

            var rows = await parser.ParseAsync(stream);

            Assert.Single(rows);
            var row = rows.First();

            Assert.Equal("ABC", row.Symbol);
            Assert.Equal(new DateTime(2023, 1, 2), row.Date.Date);
            Assert.Equal(new DateTime(2023, 2, 3), row.Expiry.Date);
            Assert.Equal(OptionRightType.Call, row.OptionType);
            Assert.Equal(100.5m, row.StrikePrice);

            Assert.Equal(1.1m, row.Open);
            Assert.Equal(2.2m, row.High);
            Assert.Equal(0.9m, row.Low);
            Assert.Equal(1.5m, row.Close);
            Assert.Equal(1.6m, row.Ltp);
            Assert.Equal(1.4m, row.SettlePrice);

            Assert.Equal(1000L, row.NumberOfContracts);
            Assert.Equal(50000.25m, row.Turnover);
            Assert.Equal(250.75m, row.PremiumTurnover);
            Assert.Equal(2000L, row.OpenInterest);

            Assert.Equal(0.5m, row.ChangeInUnderlyingValue);
            Assert.Equal(150.75m, row.UnderlyingValue);
        }

        [Fact]
        public async Task ParseAsync_ShouldSetNullsForEmptyNumericFields()
        {
            string[] fields = new string[17];
            fields[0] = "XYZ";
            fields[1] = "2023-03-04";
            fields[2] = "2023-04-05";
            fields[3] = "Put";
            fields[4] = "50"; // strike
            for (int i = 5; i <= 16; i++) fields[i] = string.Empty;

            var header = string.Join(",", Enumerable.Range(0, 17).Select(i => $"C{i}"));
            var csv = new StringBuilder();
            csv.AppendLine(header);
            csv.AppendLine(string.Join(",", fields));

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv.ToString()));
            var parser = new OptionHistoryCsvParser();

            var rows = await parser.ParseAsync(stream);

            Assert.Single(rows);
            var row = rows.First();

            Assert.Equal(50m, row.StrikePrice);

            Assert.Null(row.Open);
            Assert.Null(row.High);
            Assert.Null(row.Low);
            Assert.Null(row.Close);
            Assert.Null(row.Ltp);
            Assert.Null(row.SettlePrice);
            Assert.Null(row.NumberOfContracts);
            Assert.Null(row.Turnover);
            Assert.Null(row.PremiumTurnover);
            Assert.Null(row.OpenInterest);
            Assert.Null(row.ChangeInUnderlyingValue);
            Assert.Null(row.UnderlyingValue);
        }

        [Fact]
        public async Task ParseAsync_ShouldSkipRowsWithInsufficientColumns()
        {
            var csv = new StringBuilder();
            csv.AppendLine("a,b,c");
            csv.AppendLine("BAD,ONLY,THREE");

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv.ToString()));
            var parser = new OptionHistoryCsvParser();

            var rows = await parser.ParseAsync(stream);

            Assert.Empty(rows);
        }

        [Fact]
        public async Task ParseAsync_ShouldSkipRowsThatThrowDuringParsing()
        {
            string[] fields = new string[17];
            fields[0] = "ERR";
            fields[1] = "not-a-date";
            fields[2] = "2023-05-05";
            fields[3] = "Call";
            fields[4] = "10";
            for (int i = 5; i <= 16; i++) fields[i] = "0";

            var header = string.Join(",", Enumerable.Range(0, 17).Select(i => $"C{i}"));
            var csv = new StringBuilder();
            csv.AppendLine(header);
            csv.AppendLine(string.Join(",", fields));

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv.ToString()));
            var parser = new OptionHistoryCsvParser();

            var rows = await parser.ParseAsync(stream);

            Assert.Empty(rows);
        }
    }
}
