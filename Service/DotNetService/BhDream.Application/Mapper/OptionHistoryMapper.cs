using BhDream.Application.Dtos;
using BhDream.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Mapper
{
    public static class OptionHistoryMapper
    {
        public static OptionHistory Map(OptionHistoryCsvRow row)
        {
            return new OptionHistory
            {
                Date = row.Date,
                Open = row.Open,
                High = row.High,
                Low = row.Low,
                Close = row.Close,
                Ltp = row.Ltp,
                SettlePrice = row.SettlePrice,
                NumberOfContracts = row.NumberOfContracts,
                Turnover = row.Turnover,
                PremiumTurnover = row.PremiumTurnover,
                OpenInterest = row.OpenInterest,
                ChangeInUnderlyingValue = row.ChangeInUnderlyingValue,
                UnderlyingValue = row.UnderlyingValue,
                Contract = new OptionContract
                {
                    Expiry = row.Expiry,
                    StrikePrice = row.StrikePrice,
                    OptionType = row.OptionType,
                    Underlying = new Underlying
                    {
                        Symbol = row.Symbol
                    }
                }
            };
        }
    }
}
