using BhDream.Application.Dtos;
using BhDream.Application.ML.DTO;
using BhDream.Application.ML.Features;
using BhDream.Application.Services.Contracts;
using BhDream.Domain.Entities;
using BhDream.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BhDream.WebAPI.Controller
{
    [ApiController]
    [Route("api/ml-manager")]
    public class MlManagerController : ControllerBase
    {
        private readonly ILogger<MlManagerController> _logger;
        private readonly ITrainMlModelService _mlTrainingservice;
        public MlManagerController(ILogger<MlManagerController> logger, ITrainMlModelService mlTrainingService)
        {
            _logger = logger;
            _mlTrainingservice = mlTrainingService;
        }

        [HttpGet("get-model")]
        public IActionResult GetModels()
        {
            // Placeholder for actual model retrieval logic
            var model = new List<string>() { "KMeansClustering" };
            return Ok(model);
        }

        [HttpGet("get-features")]
        public async Task<IActionResult> GetFeaturesAsync([FromQuery] string modelName)
        {
            try
            {
                var featureOptions = await _mlTrainingservice.GetFeaturesForModel(modelName);
                var featureParameters = await _mlTrainingservice.GetParametersForModel(modelName);
                return Ok(new
                {
                    Features = featureOptions,
                    Parameters = featureParameters
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while get-features {modelName}", ex);
                return BadRequest();
            }         
        }

        [HttpPost("train-model")]
        public async Task<IActionResult> TrainModelAsync([FromBody] TrainModelRequestDto modelName)
        {
            try
            {
                await _mlTrainingservice.TrainModel(modelName);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while train-model {modelName.ModelName}", ex);
                return BadRequest();
            }
        }

        [HttpPost("trained-models")]
        public async Task<IActionResult> GetTrainedModelsPostAsync([FromBody] MlModelFilterRequestDto filterRequest)
        {
            try
            {
                var models = await _mlTrainingservice.GetFilteredModelsAsync(filterRequest);

                // Count active filters (ignoring status since it has a default)
                bool hasFilters = !string.IsNullOrEmpty(filterRequest.ModelName) ||
                                  (filterRequest.StartDateTime.HasValue && filterRequest.StartDateTime.Value != default) ||
                                  (filterRequest.EndDateTime.HasValue && filterRequest.EndDateTime.Value != default) ||
                                  (filterRequest.FeaturesPipeline != null && filterRequest.FeaturesPipeline.Count > 0) ||
                                  (filterRequest.Parameters != null && filterRequest.Parameters.Count > 0);

                // Return top 10 if no filters, top 20 if filters applied
                int limit = hasFilters ? 20 : 10;
                var results = models.Take(limit).ToList();

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching filtered ML models.");
                return BadRequest();
            }
        }
    }
}
