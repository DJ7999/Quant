using BhDream.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Abstractions.Repositories
{
    public interface IMlModelRepository
    {
        public Task<MlModel?> GetById(Guid id);
        public Task<MlModel?> GetByFeaturesAndWindow(string modelName, DateTime startDateTime, DateTime endDateTime, string features, string parameters);
        public Task StartTrainingModel(MlModel model);
    }
}
