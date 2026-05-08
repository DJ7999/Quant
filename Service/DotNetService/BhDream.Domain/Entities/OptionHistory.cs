using BhDream.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Domain.Entities
{
    
    public class OptionHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ContractId { get; set; }
        public OptionContract Contract { get; set; } = null!;

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

        public DateTime UpdatedAt {  get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; } = null;
    } 
}
