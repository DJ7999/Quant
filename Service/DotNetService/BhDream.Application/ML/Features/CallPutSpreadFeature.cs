using BhDream.Application.ML.Attributes;
using BhDream.Application.ML.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.ML.Features
{
    [TargetModel("KMeansClustering")]
    public class CallPutSpreadFeature : IModelFeature
    {
        public string FeatureName => "Call-Put Spread";

        [FeatureAttributes("Metric",InputType = UiInputType.Select, SelectOptions = new[] {"Implied Volatility", "Trading Volume"})]
        public required string Metric { get; set; }

        [FeatureAttributes("Slicing Method", 
            InputType = UiInputType.Select, 
            SelectOptions = new[] { "ATM Option", "All Active Options Average" },
            VisibleIfProperty = nameof(Metric))]
        public required string SlicingMethod { get; set; }

    }
}
