using BhDream.Application.ML.DTO;
using BhDream.Application.ML.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Services.Contracts
{
    public interface ITrainMlModelService
    {
        Task<List<FeatureMetadataDto>> GetFeaturesForModel(string modelName);
        Task TrainModel(TrainModelRequestDto modelRequest);
        Task<IMlParameters?> GetParametersForModel(string modelName);
    }
}
