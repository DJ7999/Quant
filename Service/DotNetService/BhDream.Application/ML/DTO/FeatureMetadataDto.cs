using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.ML.DTO
{
    public class FeatureMetadataDto
    {
        public required string FeatureCode { get; set; }

        // The human-friendly title displayed at the top of the form (e.g., "Moving Average")
        public required string DisplayName { get; set; }

        // The list of input configuration fields this feature needs
        public List<ParameterMetadataDto> Parameters { get; set; } = new();
    }
}
