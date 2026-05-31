using BhDream.Application.ML.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.ML.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FeatureAttributes : Attribute
    {
        public string Label { get; }

        // Changed from string to UiInputType enum
        public required UiInputType InputType { get; set; }

        public string[]? SelectOptions { get; set; } = Array.Empty<string>();
        public string? VisibleIfProperty { get; set; }
        public string[]? VisibleIfValues { get; set; }
        public FeatureAttributes(string label) => Label = label;
    }
}
