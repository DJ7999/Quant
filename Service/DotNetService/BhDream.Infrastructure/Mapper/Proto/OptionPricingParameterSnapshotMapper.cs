
using BhDream.Domain.Entities;
using BhDream.Domain.Enums;
using BhDream.Infrastructure.Protobuf;

namespace BhDream.Infrastructure.Mapper.Proto
{
    public class OptionPricingParameterSnapshotMapper
    {
        public static OptionRequestSnapshotProto ToProto(OptionPricingParameterSnapshot optionPricingParameterSnapshot)
        {
            return new OptionRequestSnapshotProto
            {
                OptionHistoryId = optionPricingParameterSnapshot.OptionHistoryId.ToString(),
                ContractId = optionPricingParameterSnapshot.OptionContractId.ToString(),
                RfrMarket = optionPricingParameterSnapshot.RfrMarket,
                RfrTenor = optionPricingParameterSnapshot.RfrTenor,
                UnderlyingValue = (double)optionPricingParameterSnapshot.UnderlyingValue,
                StrikePrice = (double)optionPricingParameterSnapshot.StrikePrice,
                OptionPriceClose = (double)optionPricingParameterSnapshot.Close,
                RiskFreeRate = (double)optionPricingParameterSnapshot.RiskFreeRateValue,
                TimeToExpiry = Math.Max(0.00001, (optionPricingParameterSnapshot.Expiry - optionPricingParameterSnapshot.Date).TotalDays / 365.0),
                IsCall = optionPricingParameterSnapshot.OptionType.Equals(OptionRightType.Call)
            };
        }
    }
}
