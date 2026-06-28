using BhDream.Application.Abstractions.Repositories;
using BhDream.Application.ML.DTO;
using BhDream.Application.ML.Features;
using BhDream.Application.ML.Parameters;
using BhDream.Application.Services.Contracts;
using BhDream.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BhDream.Application.Services
{
    public class TrainMlModelService : ITrainMlModelService
    {
        private readonly IFeatureRegistry _featureRegistry;
        private readonly ILogger<TrainMlModelService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnumerable<IMlParameters> _mlParameters;
        public TrainMlModelService(IFeatureRegistry featureRegistry, ILogger<TrainMlModelService> logger, IUnitOfWork unitOfWork, IEnumerable<IMlParameters> MlParameters) { 
            _featureRegistry = featureRegistry;
            _logger = logger; 
            _unitOfWork = unitOfWork;
            _mlParameters = MlParameters;
        }

        public async Task<List<FeatureMetadataDto>> GetFeaturesForModel(string modelName)
        {
            return await Task.FromResult(_featureRegistry.GetFeaturesForModel(modelName));
        }

        // Cleaner and faster execution
        public Task<IMlParameters?> GetParametersForModel(string modelName)
        {
            var strategy = _mlParameters.FirstOrDefault(p => string.Equals(p.ModelName, modelName, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(strategy);
        }

        public async Task TrainModel(TrainModelRequestDto modelRequest)
        {
            _logger.LogInformation("Starting training for model {ModelName}", modelRequest.ModelName);
            MappedFeaturePayload features = _featureRegistry.MapPayloadToFeatures(modelRequest);
            string featureString = JsonSerializer.Serialize(features);
            string parametersString = JsonSerializer.Serialize(modelRequest.Parameters);
            (var minDate, var maxDate) = await _unitOfWork.OptionHistoryRepository.GetFirstAndLastDate();
            var modelStartDate = new DateTime(minDate.Year, minDate.Month, 1);
            var modelEndDate = modelStartDate.AddMonths(modelRequest.ModelTrainingWindowInMonths).AddDays(-1);
            while (modelEndDate <= maxDate)
            {
                _logger.LogInformation($"Model Start : {modelStartDate} , Model End : {modelEndDate} Current");
                MlModel? model = await _unitOfWork.MlModelRepository.GetByFeaturesAndWindow(modelRequest.ModelName, modelStartDate, modelEndDate, featureString, parametersString);

                //check if model is already trained for the given feature
                if (model != null && model.Status == Domain.Enums.MlTrainingStatus.Trained)
                {
                    _logger.LogInformation($"Trained Model already exists for the given features and window {modelRequest.ModelName}: {modelStartDate} - {modelEndDate}. Model ID: {model.Id}");
                }
                else if (model != null && model.Status == Domain.Enums.MlTrainingStatus.InProgress)
                {
                    _logger.LogInformation($"Model training Inprogress {modelRequest.ModelName}: {modelStartDate} - {modelEndDate}. Model ID: {model.Id}");
                    
                }
                else if (model != null && model.Status == Domain.Enums.MlTrainingStatus.Pending)
                {
                    _logger.LogInformation($"Model training is pending in quque {modelRequest.ModelName}: {modelStartDate} - {modelEndDate}. Model ID: {model.Id}");

                }
                else if (model != null && model.Status == Domain.Enums.MlTrainingStatus.Failed)
                {
                    _logger.LogInformation($"Previous model training failed for {modelRequest.ModelName}: {modelStartDate} - {modelEndDate}.");
                }
                else
                {

                    model = model ?? new MlModel();
                    model.ModelName = modelRequest.ModelName;
                    model.StartDateTime = modelStartDate;
                    model.EndDateTime = modelEndDate;
                    model.Features = featureString;
                    model.Parameters = parametersString;
                    model.Status = Domain.Enums.MlTrainingStatus.Pending;
                    model.LastUpdatedAt = DateTime.Now;

                    await _unitOfWork.MlModelRepository.StartTrainingModel(model);

                    await _unitOfWork.SaveChangesAsync();
                }
                
                modelStartDate = modelStartDate.AddMonths(1);
                modelEndDate = modelStartDate.AddMonths(modelRequest.ModelTrainingWindowInMonths).AddDays(-1);
                _logger.LogInformation($"Model Start : {modelStartDate} , Model End : {modelEndDate} Updated");
            }
        }
    }
}
