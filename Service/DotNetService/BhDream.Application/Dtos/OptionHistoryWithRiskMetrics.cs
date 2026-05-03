using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Dtos
{
    public class OptionHistoryWithRiskMetricsDto
    {
        public DateTime Date =>OptionHistory.Date;
        public required OptionHistoryDto OptionHistory { get; set; }
        public OptionRiskMetricDto? RiskMetrics { get; set; }
    }
}
