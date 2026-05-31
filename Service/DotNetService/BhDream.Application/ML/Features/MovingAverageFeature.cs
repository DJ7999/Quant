using BhDream.Application.ML.Attributes;
using BhDream.Application.ML.Enum;

namespace BhDream.Application.ML.Features
{
    [TargetModel("KMeansClustering")]
    public class MovingAverageFeature : IModelFeature
    {
        public string FeatureName => "Moving Average";

        [FeatureAttributes("Rolling Window Size", InputType = UiInputType.Number)]
        public required int RollingWindowSize { get; set; }

        [FeatureAttributes("Target Metric", 
            InputType = UiInputType.Select,
            SelectOptions = new[] { "Log Return", "Sigma", "Implied Volatility", "Underlying Volume", "Option Contract Volume"})]
        public required string TargetMetric { get; set; }

        [FeatureAttributes("Option Type", 
            InputType = UiInputType.Select, 
            SelectOptions = new[] {"Call","Put","Combined"},
            VisibleIfProperty = nameof(TargetMetric),
            VisibleIfValues = new[] { "Implied Volatility", "Option Contract Volume" })]
        public string? OptionType { get; set; }

        [FeatureAttributes("Slicing Method", 
            InputType = UiInputType.Select, 
            SelectOptions = new[] {"ATM Option", "All Active Options Average"},
            VisibleIfProperty = nameof(OptionType),
            VisibleIfValues = new[] { "Implied Volatility", "Option Contract Volume" })]
        public string? SlicingMethod { get; set; }

    }
}
