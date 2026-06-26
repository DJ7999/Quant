using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.ML.Parameters
{
    public class MlParamDto
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public string InputType { get; set; }

        public string? DefaultValue { get; set; }

        public string? Minvalue { get; set; }
        public bool IsRequired { get; set; }
    }
}
