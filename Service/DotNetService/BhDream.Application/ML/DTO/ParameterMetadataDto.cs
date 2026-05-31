using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.ML.DTO
{
    public class ParameterMetadataDto
    {
        public required string Name { get; set; }

        // The UI text label shown next to the input field (e.g., "Rolling Window Size")
        public required string Label { get; set; }

        // Controls how the UI renders it (e.g., "number" or "select")
        public required string Type { get; set; }

        // Optional standard fallback value if the user hasn't typed anything yet
        public object? DefaultValue { get; set; }

        // The lowercase name of the property this field's visibility depends on
        public string? VisibleIfProperty { get; set; }

        // The string values that trigger this field to show up on the screen
        public List<string> VisibleIfValues { get; set; } = new();

        // Streamlined: A simple list of dropdown choices (e.g., ["Call", "Put", "Combined"])
        // This is empty if the Type is "number"
        public List<string> Options { get; set; } = new();
    }
}
