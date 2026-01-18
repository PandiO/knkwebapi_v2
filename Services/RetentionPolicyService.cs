using System;
using System.Threading;
using System.Threading.Tasks;
using knkwebapi_v2.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace knkwebapi_v2.Services
{
    /// <summary>
    /// Background service that enforces retention policies.
    /// - Deletes completed FormSubmissionProgress records older than 14 days.
    /// Runs once per day at startup and then every 24 hours.
    /// </summary>
    public class RetentionPolicyService : BackgroundService
    {
        private readonly IFormSubmissionProgressRepository _formProgressRepository;
        private readonly ILogger<RetentionPolicyService> _logger;
        private readonly int _retentionDays;
        private readonly TimeSpan _runInterval;

        public RetentionPolicyService(
            IFormSubmissionProgressRepository formProgressRepository,
            ILogger<RetentionPolicyService> logger,
            int retentionDays = 14,
            TimeSpan? runInterval = null)
        {
            _formProgressRepository = formProgressRepository;
            _logger = logger;
            _retentionDays = retentionDays;
            _runInterval = runInterval ?? TimeSpan.FromHours(24); // Default: run daily
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "RetentionPolicyService started. Will clean up records older than {RetentionDays} days every {Interval}",
                _retentionDays,
                _runInterval.TotalHours);

            // Run immediately on startup
            await RunCleanupAsync(stoppingToken);

            // Schedule recurring cleanup
            using (var timer = new PeriodicTimer(_runInterval))
            {
                try
                {
                    while (await timer.WaitForNextTickAsync(stoppingToken))
                    {
                        await RunCleanupAsync(stoppingToken);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("RetentionPolicyService stopping");
                }
            }
        }

        private async Task RunCleanupAsync(CancellationToken cancellationToken)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-_retentionDays);
                
                _logger.LogInformation(
                    "Running retention policy cleanup. Deleting FormSubmissionProgress records completed before {CutoffDate}",
                    cutoffDate);

                int deletedCount = await _formProgressRepository.DeleteCompletedOlderThanAsync(cutoffDate);

                _logger.LogInformation(
                    "Retention policy cleanup completed. Deleted {Count} completed form submissions",
                    deletedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running retention policy cleanup");
            }
        }
    }
}
