using BhDream.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Dtos
{
    public class OptionHistoryCsvRow
    {
        public string Symbol { get; set; } = null!;
        public DateTime Date { get; set; }
        public DateTime Expiry { get; set; }
        public OptionRightType OptionType { get; set; }
        public decimal StrikePrice { get; set; }

        public decimal? Open { get; set; }
        public decimal? High { get; set; }
        public decimal? Low { get; set; }
        public decimal? Close { get; set; }

        public long? OpenInterest { get; set; }
        public decimal? Ltp { get; set; }

        public long? NumberOfContracts { get; set; }
        public decimal? Turnover { get; set; }
        public decimal? PremiumTurnover { get; set; }

        public decimal? SettlePrice { get; set; }

        public decimal? UnderlyingValue { get; set; }

        public decimal? ChangeInUnderlyingValue { get; set; }
    }
}
