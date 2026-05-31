using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.ML.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TargetModelAttribute : Attribute
    {
        public string ModelName { get; }
        public TargetModelAttribute(string modelName) => ModelName = modelName;
    }
}
