using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BhDream.Application.Hypothesis.DTO
{
    public class HypothesisBacktestRequestDto
    {
        [JsonPropertyName("modelGuid")]
        public Guid ModelGuid { get; set; }

        [JsonPropertyName("strategyConfigs")]
        public List<StrategyConfigDto> StrategyConfigs { get; set; } = new();
    }

    public class StrategyConfigDto
    {
        [JsonPropertyName("variantId")]
        public required string VariantId { get; set; }

        [JsonPropertyName("strategyName")]
        public required string StrategyName { get; set; }

        [JsonPropertyName("legs")]
        public List<LegConfigDto> Legs { get; set; } = new();
    }

    public class LegConfigDto
    {
        [JsonPropertyName("legType")]
        public required string LegType { get; set; } // "Option" or "Underlying"

        [JsonPropertyName("isLong")]
        public bool IsLong { get; set; }

        [JsonPropertyName("isCall")]
        public bool IsCall { get; set; }

        [JsonPropertyName("moneynessOffset")]
        public double MoneynessOffset { get; set; }

        [JsonPropertyName("expiryTenorDays")]
        public int ExpiryTenorDays { get; set; }
    }
}
