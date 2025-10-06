// In Services/DataPurgeService.cs

namespace RealTimeAnalytics.Api.Services
{
    public class DataPurgeService : BackgroundService
    {
        private readonly ILogger<DataPurgeService> _logger;
        private readonly FilePersistenceService _persistenceService;

        public DataPurgeService(ILogger<DataPurgeService> logger, FilePersistenceService persistenceService)
        {
            _logger = logger;
            _persistenceService = persistenceService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Wait for one hour before running the next purge.
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);

                try
                {
                    _logger.LogInformation("Running daily data purge job...");
                    _persistenceService.PurgeOldData(TimeSpan.FromHours(24));
                    _logger.LogInformation("Data purge job completed successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during the data purge job.");
                }
            }
        }
    }
}