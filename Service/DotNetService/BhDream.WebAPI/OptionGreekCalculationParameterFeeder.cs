
using BhDream.Application.Abstractions.ExternalServices;
using BhDream.Application.Services.Contracts;

namespace BhDream.WebAPI
{
    public class OptionGreekCalculationParameterFeeder : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptionPricingDispatcher _dispatcher;
        private readonly ILogger<OptionGreekCalculationParameterFeeder> _logger;
        
        public OptionGreekCalculationParameterFeeder(
            IServiceProvider serviceProvider,
            IOptionPricingDispatcher dispatcher,
            ILogger<OptionGreekCalculationParameterFeeder> logger)
        {
            _serviceProvider = serviceProvider;
            _dispatcher = dispatcher;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Option Pricing Parameter Feeder Started.");
            int counter = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        IOptionProcessingService optionProcessingService = scope.ServiceProvider.GetRequiredService<IOptionProcessingService>();
                        var unprocessedOptionParameterSnapshots = await optionProcessingService.GetUnProcessedOptionParameters(1000);
                        if (unprocessedOptionParameterSnapshots != null && unprocessedOptionParameterSnapshots.Any())
                        {
                            var batchId = Guid.NewGuid();
                            
                            await _dispatcher.Dispatch(unprocessedOptionParameterSnapshots, batchId);
                            _logger.LogInformation($"Dispatched Batches.= {counter++}");
                        }
                        else
                        {
                            // NO DATA FOUND: Wait a bit so we don't spam the database
                            await Task.Delay(100000, stoppingToken);
                        }

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while feeding option pricing parameters.");
                }
                
            }
        }
    }
}
