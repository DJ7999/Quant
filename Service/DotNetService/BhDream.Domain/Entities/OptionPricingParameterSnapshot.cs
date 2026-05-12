using BhDream.Domain.Enums;

namespace BhDream.Domain.Entities
{
    public class OptionPricingParameterSnapshot
    {

        public Guid OptionHistoryId { get; set; }
        public Guid OptionContractId { get; set; }
        public string RfrMarket { get; set; }
        public string RfrTenor { get; set; }
        public int RfrTenorDays { get; set; }
        public decimal UnderlyingValue { get; set; }
        public decimal StrikePrice { get; set; }
        public decimal Close { get; set; }
        public DateTime Date { get; set; }
        public DateTime Expiry { get; set; }
        public OptionRightType OptionType { get; set; }
        public decimal RiskFreeRateValue { get; set; }
    }
}
