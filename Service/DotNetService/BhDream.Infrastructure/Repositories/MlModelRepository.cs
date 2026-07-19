using BhDream.Application.Abstractions.Repositories;
using BhDream.Domain.Entities;
using BhDream.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
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

        public async Task<List<MlModel>> GetFilteredModels(MlModel model)
        {
            try
            {
                string sql = "SELECT * FROM ml_models WHERE 1=1";
                var parameters = new List<object>();

                if (!string.IsNullOrEmpty(model.ModelName))
                {
                    sql += $" AND \"ModelName\" = {{{parameters.Count}}}";
                    parameters.Add(model.ModelName);
                }

                if (model.StartDateTime != default)
                {
                    sql += $" AND \"StartDateTime\" >= {{{parameters.Count}}}";
                    parameters.Add(model.StartDateTime);
                }

                if (model.EndDateTime != default)
                {
                    sql += $" AND \"EndDateTime\" <= {{{parameters.Count}}}";
                    parameters.Add(model.EndDateTime);
                }

                if (!string.IsNullOrEmpty(model.Features))
                {
                    if (model.Features.Contains('|'))
                    {
                        var parts = model.Features.Split('|');
                        foreach (var part in parts)
                        {
                            if (part.Contains(':'))
                            {
                                var pParts = part.Split(':');
                                var pKey = pParts[0];
                                var pVal = pParts[1];
                                sql += $" AND (CAST(\"Features\" AS text) LIKE {{{parameters.Count}}} OR CAST(\"Features\" AS text) LIKE {{{parameters.Count + 1}}} OR CAST(\"Features\" AS text) LIKE {{{parameters.Count + 2}}} OR CAST(\"Features\" AS text) LIKE {{{parameters.Count + 3}}})";
                                parameters.Add($"%\"{pKey}\":{pVal}%");
                                parameters.Add($"%\"{pKey}\": {pVal}%");
                                parameters.Add($"%\"{pKey}\":\"{pVal}\"%");
                                parameters.Add($"%\"{pKey}\": \"{pVal}\"%");
                            }
                            else
                            {
                                sql += $" AND CAST(\"Features\" AS text) LIKE {{{parameters.Count}}}";
                                parameters.Add($"%{part}%");
                            }
                        }
                    }
                    else
                    {
                        sql += $" AND CAST(\"Features\" AS text) LIKE {{{parameters.Count}}}";
                        parameters.Add($"%{model.Features}%");
                    }
                }

                if (!string.IsNullOrEmpty(model.Parameters))
                {
                    if (model.Parameters.Contains(':'))
                    {
                        var parts = model.Parameters.Split(':');
                        var pKey = parts[0];
                        var pVal = parts[1];
                        sql += $" AND (CAST(\"Parameters\" AS text) LIKE {{{parameters.Count}}} OR CAST(\"Parameters\" AS text) LIKE {{{parameters.Count + 1}}} OR CAST(\"Parameters\" AS text) LIKE {{{parameters.Count + 2}}} OR CAST(\"Parameters\" AS text) LIKE {{{parameters.Count + 3}}})";
                        parameters.Add($"%\"{pKey}\":{pVal}%");
                        parameters.Add($"%\"{pKey}\": {pVal}%");
                        parameters.Add($"%\"{pKey}\":\"{pVal}\"%");
                        parameters.Add($"%\"{pKey}\": \"{pVal}\"%");
                    }
                    else
                    {
                        sql += $" AND CAST(\"Parameters\" AS text) LIKE {{{parameters.Count}}}";
                        parameters.Add($"%{model.Parameters}%");
                    }
                }

                sql += $" AND \"Status\" = {{{parameters.Count}}}";
                parameters.Add((int)model.Status);

                var results = await _dbContext.MlModels
                    .FromSqlRaw(sql, parameters.ToArray())
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} ML models matching the filter criteria.", results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving filtered ML models.");
                throw;
            }
        }
    }
}