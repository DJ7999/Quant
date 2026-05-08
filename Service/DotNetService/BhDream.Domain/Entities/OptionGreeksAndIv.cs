using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Domain.Entities
{
    public class OptionGreeksAndIv
    {
        public Guid OptionHistoryId {  get; set; }
        public OptionHistory OptionHistory { get; set; }

        public Guid ContractId { get; set; }
        public OptionContract Contract { get; set; } = null!;
        public string RfrMarket { get; set; }
        public string RfrTenor { get; set; }
        public double Delta { get; set; }
        public double Theta { get; set; }
        public double Gamma { get; set; }
        public double Vega { get; set; }
        public double Rho { get; set; }
        public double Vomma { get; set; }
        public double ImpliedVolatility { get; set; }
        public DateTime CalculatedAt { get; set; }
    }
}
