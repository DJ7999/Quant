using BhDream.Application.Dtos;
using BhDream.Application.ML.DTO;
using BhDream.Application.ML.Features;
using BhDream.Application.Services.Contracts;
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
    }
}
