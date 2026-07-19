using System.Collections.Generic;

namespace BhDream.Application.Hypothesis.DTO
{
    public class StrategyMetadataDto
    {
        public List<StrategyTemplateDto> Strategies { get; set; } = new();
        public List<LegParameterMetadataDto> LegParameters { get; set; } = new();
    }

    public class StrategyTemplateDto
    {
        public required string StrategyName { get; set; } // e.g., "CoveredCall"
        public required string DisplayName { get; set; }  // e.g., "Covered Call"
        public required string Description { get; set; }
        public List<LegTemplateDto> DefaultLegs { get; set; } = new();
    }

    public class LegTemplateDto
    {
        public required string LegType { get; set; } // "Option" or "Underlying"
        public bool IsLong { get; set; }
        public bool IsCall { get; set; }
        public double MoneynessOffset { get; set; }
        public int ExpiryTenorDays { get; set; }
    }

    public class LegParameterMetadataDto
    {
        public required string Key { get; set; } // e.g., "isLong"
        public required string Label { get; set; } // e.g., "Direction"
        public required string InputType { get; set; } // "boolean", "number", "select"
        public object? DefaultValue { get; set; }
        public List<string>? Options { get; set; }
        public string? VisibleIfProperty { get; set; }
        public List<object>? VisibleIfValues { get; set; }
    }
}
