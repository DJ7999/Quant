using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace BhDream.Application.ML.DTO
{
    public class TrainModelRequestDto
    {
        public required string ModelName { get; set; } // e.g., "KMeansClustering"

        public List<FeatureInstancePayloadDto> FeaturesPipeline { get; set; } = new();

        public JsonObject Parameters { get; set; }

        public int ModelTrainingWindowInMonths { get; set; } = 12;

    }
}
