using BhDream.Application.Abstractions.Repositories;
using BhDream.Domain.Entities;
using BhDream.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BhDream.Infrastructure.Repositories
{
    public class MlModelRepository : IMlModelRepository
    {
        private readonly QuantDbContext _dbContext;
        private readonly ILogger<MlModelRepository> _logger;

        public MlModelRepository(QuantDbContext quantDbContext, ILogger<MlModelRepository> logger)
        {
            _dbContext = quantDbContext;
            _logger = logger;
        }

        public Task StartTrainingModel(MlModel model)
        {
            // If the Guid is empty or default, it's definitely a new insert
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                _dbContext.MlModels.Add(model);
                _logger.LogInformation("Adding brand new model record.");
            }
            else
            {
                // If it already has an ID, instruct EF Core to track it as an update
                _dbContext.MlModels.Update(model);
                _logger.LogInformation("Updating existing model record with ID: {Id}", model.Id);
            }

            return Task.CompletedTask;
        }

        public async Task<MlModel?> GetByFeaturesAndWindow(string modelName, DateTime startDateTime, DateTime endDateTime, string features, string parameters)
        {
            try
            {
                return await _dbContext.MlModels
                    .FirstOrDefaultAsync(m => m.ModelName == modelName
                                           && m.StartDateTime == startDateTime
                                           && m.EndDateTime == endDateTime
                                           && m.Features == features
                                           && m.Parameters == parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encountered while looking up existing model execution parameters.");
                throw;
            }
        }

        public async Task<MlModel?> GetById(Guid id)
        {
            try
            {
                return await _dbContext.MlModels.FirstOrDefaultAsync(m => m.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ML model execution with ID: {Id}", id);
                throw;
            }
        }
    }
}