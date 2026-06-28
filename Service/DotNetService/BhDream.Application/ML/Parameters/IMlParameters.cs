using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.ML.Parameters
{
    public interface IMlParameters
    {
        string ModelName { get; }
        List<MlParamDto> Parameters { get; }
    }
}
