using System;
using BhDream.Application.Dtos;
using BhDream.Application.Mapper;
using Xunit;

namespace BhDream.Application.Tests.MapperTests
{
    public class OptionHistoryMapperTests
    {
        [Fact]
        public void Map_ShouldMapAllFields()
        {
            var row = new OptionHistoryCsvRow
            {
                Symbol = "SYM",
                Date = new DateTime(2023,1,2),
                Expiry = new DateTime(2023,2,3),
                OptionType = Domain.Enums.OptionRightType.Put,
                StrikePrice = 123.45m,
                Open = 1.1m,
                High = 2.2m,
                Low = 0.9m,
                Close = 1.5m,
                Ltp = 1.6m,
                SettlePrice = 1.4m,
                NumberOfContracts = 1000,
                Turnover = 50000.25m,
                PremiumTurnover = 250.75m,
                OpenInterest = 2000,
                ChangeInUnderlyingValue = 0.5m,
                UnderlyingValue = 150.75m
            };

            var entity = OptionHistoryMapper.Map(row);

            Assert.Equal(row.Date, entity.Date);
            Assert.Equal(row.Open, entity.Open);
            Assert.Equal(row.High, entity.High);
            Assert.Equal(row.Low, entity.Low);
            Assert.Equal(row.Close, entity.Close);
            Assert.Equal(row.Ltp, entity.Ltp);
            Assert.Equal(row.SettlePrice, entity.SettlePrice);
            Assert.Equal(row.NumberOfContracts, entity.NumberOfContracts);
            Assert.Equal(row.Turnover, entity.Turnover);
            Assert.Equal(row.PremiumTurnover, entity.PremiumTurnover);
            Assert.Equal(row.OpenInterest, entity.OpenInterest);
            Assert.Equal(row.ChangeInUnderlyingValue, entity.ChangeInUnderlyingValue);
            Assert.Equal(row.UnderlyingValue, entity.UnderlyingValue);

            Assert.NotNull(entity.Contract);
            Assert.Equal(row.Expiry, entity.Contract.Expiry);
            Assert.Equal(row.StrikePrice, entity.Contract.StrikePrice);
            Assert.Equal(row.OptionType, entity.Contract.OptionType);
            Assert.NotNull(entity.Contract.Underlying);
            Assert.Equal(row.Symbol, entity.Contract.Underlying.Symbol);
        }
    }
}
