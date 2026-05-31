using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace BhDream.Application.ML.DTO
{
    public class FeatureInstancePayloadDto
    {
        public required string FeatureCode { get; set; } // e.g., "movingaverage", "callputspread"

        // Using JsonObject allows us to capture whatever dynamic keys/values the frontend sends
        // without breaking serialization before we figure out the C# target type.
        public required JsonObject Parameters { get; set; }
    }
}
