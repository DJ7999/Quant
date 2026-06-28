using System;
using System.Collections.Generic;
using System.Linq;

namespace BhDream.Application.ML.Features
{
    public class MappedFeaturePayload
    {
        // Private backing fields to store the raw data
        private readonly List<CallPutSpreadFeature> _callPutSpreadFeatures = new();
        private readonly List<MovingAverageFeature> _movingAverageFeatures = new();

        // 🎯 Serializer targets: These intercept reads and return items in a strict, predictable order
        public List<CallPutSpreadFeature> CallPutSpreadFeatures => _callPutSpreadFeatures
            .OrderBy(f => f.FeatureName)
            .ThenBy(f => f.Metric)
            .ThenBy(f => f.SlicingMethod)
            .ToList();

        public List<MovingAverageFeature> MovingAverageFeatures => _movingAverageFeatures
            .OrderBy(f => f.FeatureName)
            .ThenBy(f => f.TargetMetric)
            .ThenBy(f => f.RollingWindowSize)
            .ThenBy(f => f.OptionType)
            .ToList();

        public void AddFeature(IModelFeature feature)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));

            switch (feature)
            {
                case CallPutSpreadFeature cps:
                    _callPutSpreadFeatures.Add(cps);
                    break;

                case MovingAverageFeature ma:
                    _movingAverageFeatures.Add(ma);
                    break;

                default:
                    throw new ArgumentException($"Unsupported feature type: {feature.GetType().Name}", nameof(feature));
            }
        }
    }
}