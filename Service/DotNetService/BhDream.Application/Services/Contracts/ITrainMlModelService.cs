using BhDream.Application.ML.DTO;
using BhDream.Application.ML.Parameters;
using BhDream.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BhDream.Application.Services.Contracts
{
    public interface ITrainMlModelService
    {
        Task<List<FeatureMetadataDto>> GetFeaturesForModel(string modelName);
        Task TrainModel(TrainModelRequestDto modelRequest);
        Task<IMlParameters?> GetParametersForModel(string modelName);
        Task<List<MlModel>> GetFilteredModelsAsync(MlModelFilterRequestDto filterRequest);
    }
}
