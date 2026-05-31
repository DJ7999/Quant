using BhDream.Application.ML.Attributes;
using BhDream.Application.ML.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BhDream.Application.ML.Features
{
    public class FeatureRegistry : IFeatureRegistry
    {
        // 1. Existing storage for UI parameter blueprints
        private readonly Dictionary<string, List<FeatureMetadataDto>> _modelFeaturesParameters = new Dictionary<string, List<FeatureMetadataDto>>();

        // 2. NEW: Lookup cache pairing the exact FeatureCode (featureType.Name) string to its structural C# System.Type metadata
        private readonly Dictionary<string, Type> _featureClassCache = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        // 3. Shared JSON options configuration context
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public FeatureRegistry()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Safely pull the exact project assembly containing your ML Features using a marker type
            var targetAssembly = Assembly.GetAssembly(typeof(IModelFeature));
            if (targetAssembly == null) return;

            var featureTypes = targetAssembly.GetTypes()
                .Where(t => typeof(IModelFeature).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var featureType in featureTypes)
            {
                var targetModelAttribute = featureType.GetCustomAttributes<TargetModelAttribute>();
                if (!targetModelAttribute.Any()) continue;

                string displayName = FormatFeatureName(featureType.Name);

                // --- CACHE THE TYPE LINK HERE ---
                // Stores the Type indexed by the exact FeatureCode we expose to the React frontend client loop
                _featureClassCache[featureType.Name] = featureType;

                var featureMetadata = new FeatureMetadataDto
                {
                    FeatureCode = featureType.Name, // e.g. "MovingAverageFeature" or "CallPutSpreadFeature"
                    DisplayName = displayName,
                    Parameters = new List<ParameterMetadataDto>() // Ensuring this is initialized cleanly
                };

                foreach (var prop in featureType.GetProperties())
                {
                    var attr = prop.GetCustomAttribute<FeatureAttributes>();
                    if (attr == null) continue;

                    featureMetadata.Parameters.Add(new ParameterMetadataDto
                    {
                        Name = prop.Name.ToLower(),
                        Label = attr.Label,
                        Type = attr.InputType.ToString().ToLower(),
                        VisibleIfProperty = attr.VisibleIfProperty?.ToLower(),
                        VisibleIfValues = attr.VisibleIfValues?.Select(v => v.ToLower()).ToList() ?? new List<string>(),
                        Options = attr.SelectOptions?.ToList() ?? new List<string>()
                    });
                }

                foreach (var attr in targetModelAttribute)
                {
                    if (!_modelFeaturesParameters.ContainsKey(attr.ModelName))
                        _modelFeaturesParameters[attr.ModelName] = new List<FeatureMetadataDto>();

                    _modelFeaturesParameters[attr.ModelName].Add(featureMetadata);
                }
            }
        }

        public List<FeatureMetadataDto> GetFeaturesForModel(string modelName)
        {
            return _modelFeaturesParameters.TryGetValue(modelName, out var features) ? features : new List<FeatureMetadataDto>();
        }

        /// <summary>
        /// Instantiates and strongly types an incoming dynamic workspace payload by reading from our cached internal mappings.
        /// </summary>
        public List<IModelFeature> MapPayloadToFeatures(TrainModelRequestDto requestDto)
        {
            var instantiatedPipeline = new List<IModelFeature>();

            if (requestDto?.FeaturesPipeline == null || !requestDto.FeaturesPipeline.Any())
            {
                return instantiatedPipeline;
            }

            foreach (var instance in requestDto.FeaturesPipeline)
            {
                // Look up target feature type using the incoming featureCode matching our cached type name keys
                if (!_featureClassCache.TryGetValue(instance.FeatureCode, out var targetType))
                {
                    throw new KeyNotFoundException(
                        $"ML Pipeline compilation error: Feature processing code component identification '{instance.FeatureCode}' is missing inside assembly schema tracking mappings."
                    );
                }

                // Serialize dynamic JsonObject configurations down to a string block
                string rawJsonText = instance.Parameters.ToJsonString();

                // Polymorphically inflate to the explicit destination class type mapping context
                var instantiatedFeature = (IModelFeature)JsonSerializer.Deserialize(rawJsonText, targetType, _jsonOptions)!;

                instantiatedPipeline.Add(instantiatedFeature);
            }

            return instantiatedPipeline;
        }
        

        private static string FormatFeatureName(string typeName)
        {
            string cleanName = typeName.Replace("Feature", "");
            return string.Concat(cleanName.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }
    }
}