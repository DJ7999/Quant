using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.ML.Parameters
{
    public class KMeansParameters : IMlParameters
    {
        public string ModelName => "KMeansClustering";

        public List<MlParamDto> Parameters => new List<MlParamDto>
        {
            new MlParamDto
            {
                Key = "k",
                Label = "Number of Clusters",
                InputType = "Integer",
                Minvalue = "3",
                IsRequired = true
            },
            new MlParamDto
            {
                Key = "max_itr",
                Label = "Maximum Iterations",
                InputType = "Integer",
                DefaultValue = "300",
                Minvalue = "1",
                IsRequired = true
            }
        };
    }
}
