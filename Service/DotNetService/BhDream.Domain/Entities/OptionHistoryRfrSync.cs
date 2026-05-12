using BhDream.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Domain.Entities
{
    public class OptionHistoryRfrSync
    {
        public DateTime Date { get; set; }
        public Guid OptionHistoryId { get; set; }
        public OptionHistory OptionHistory { get; set; }
        public string RfrMarket { get; set; }
        public string RfrTenor { get; set; }
        public RiskFreeRate RiskFreeRate { get; set; }
        public ProcessingStatus ProcessingStatus { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime StatusChangedAt { get; set; }
    }
}
