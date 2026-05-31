using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.ML.DTO
{
    public class TrainModelRequestDto
    {
        public required string ModelName { get; set; } // e.g., "KMeansClustering"
        public List<FeatureInstancePayloadDto> FeaturesPipeline { get; set; } = new();
    }
}
