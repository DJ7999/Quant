using BhDream.Application.Dtos;
using BhDream.Application.ML.DTO;
using BhDream.Application.ML.Features;
using Microsoft.AspNetCore.Mvc;

namespace BhDream.WebAPI.Controller
{
    [ApiController]
    [Route("api/ml-manager")]
    public class MlManagerController : ControllerBase
    {
        private readonly ILogger<MlManagerController> _logger;
        private readonly IFeatureRegistry _featureRegistry;
        public MlManagerController(ILogger<MlManagerController> logger, IFeatureRegistry featureRegistry)
        {
            _logger = logger;
            _featureRegistry = featureRegistry;
        }

        [HttpGet("get-model")]
        public IActionResult GetModels()
        {
            // Placeholder for actual model retrieval logic
            var model = new List<string>() { "KMeansClustering" };
            return Ok(model);
        }

        [HttpGet("get-features")]
        public IActionResult GetFeatures([FromQuery] string modelName)
        {
            try
            {
                var featureOptions =  _featureRegistry.GetFeaturesForModel(modelName);
                return Ok(featureOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while get-features {modelName}", ex);
                return BadRequest();
            }         
        }

        [HttpPost("train-model")]
        public IActionResult GetFeatures([FromBody] TrainModelRequestDto modelName)
        {
            try
            {
                var featureOptions = _featureRegistry.MapPayloadToFeatures(modelName);
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
