using BhDream.Application.ML.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.ML.Features
{
    public interface IFeatureRegistry
    {
        List<FeatureMetadataDto> GetFeaturesForModel(string modelName);
        MappedFeaturePayload MapPayloadToFeatures(TrainModelRequestDto requestDto);
    }
}
