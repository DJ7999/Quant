using BhDream.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace BhDream.Application.ML.DTO
{
    public class MlModelFilterRequestDto
    {
        public string? ModelName { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public List<FeatureInstancePayloadDto>? FeaturesPipeline { get; set; }

        public JsonObject? Parameters { get; set; }

        public MlTrainingStatus? Status { get; set; }
    }
}
