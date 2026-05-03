using BhDream.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Dtos
{
    public class OptionHistoryDto
    {
        public DateTime Date { get; set; }

        public decimal? Open { get; set; }
        public decimal? High { get; set; }
        public decimal? Low { get; set; }
        public decimal? Close { get; set; }
        public decimal? Ltp { get; set; }

        public decimal? SettlePrice { get; set; }

        public long? NumberOfContracts { get; set; }

        public decimal? Turnover { get; set; }
        public decimal? PremiumTurnover { get; set; }

        public long? OpenInterest { get; set; }

        public decimal? ChangeInUnderlyingValue { get; set; }

        public decimal? UnderlyingValue { get; set; }
    }
}
