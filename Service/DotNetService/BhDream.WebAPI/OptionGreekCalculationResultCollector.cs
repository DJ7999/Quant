
using BhDream.Application.Abstractions.ExternalServices;
using BhDream.Application.Services.Contracts;
using BhDream.Domain.Entities;


namespace BhDream.WebAPI
{
    public class OptionGreekCalculationResultCollector : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptionGreeksResultReceiver _receiver;
        private readonly ILogger<OptionGreekCalculationResultCollector> _logger;

        public OptionGreekCalculationResultCollector(
            IServiceProvider serviceProvider,
            ILogger<OptionGreekCalculationResultCollector> logger,
            IOptionGreeksResultReceiver optionGreeksResultReceiver)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _receiver = optionGreeksResultReceiver;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Option Pricing Calculated Greeks Worker started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        List<OptionGreeksAndIv> results = await _receiver.Receive();
                        if (results != null && results.Any())
                        {
                            IOptionProcessingService optionProcessingService = scope.ServiceProvider.GetRequiredService<IOptionProcessingService>();
                            await optionProcessingService.SaveOptionGreeksResults(results);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while collecting option pricing results.");

                }
            }
        }
    }
}
