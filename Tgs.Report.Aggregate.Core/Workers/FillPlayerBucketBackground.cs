using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Tgs.Service.PlayerBuckets;

namespace Tgs.Report.Aggregate.Core.Workers;

public class FillPlayerBucketBackground : BackgroundService
{
    private readonly ILogger<FillPlayerBucketBackground> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IFillPlayerBucketTaskQueue _taskQueue;

    public FillPlayerBucketBackground(IFillPlayerBucketTaskQueue taskQueue,
        IServiceProvider serviceProvider,
        ILogger<FillPlayerBucketBackground> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Fill player bucket queued hosted service is running.");

        await BackgroundProcessingAsync(stoppingToken);
    }

    private async Task BackgroundProcessingAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var playerBucketClient = scope.ServiceProvider.GetRequiredService<IPlayerBucketClient>();

                var request = await _taskQueue.DequeueAsync(stoppingToken);

                await Policy.Handle<Exception>()
                    .RetryAsync(
                        3,
                        (exception, retryCount) => _logger.LogError(
                            "PlayerCode:{model.PlayerCode} Turnover:{model.Turnover} retryCount:{retryCount} Exception:{exception}",
                            request.PlayerCode,
                            request.Turnover,
                            retryCount,
                            exception))
                    .ExecuteAsync(
                        async () =>
                            await playerBucketClient.FillInBucketAsync(
                                request.PlayerCode,
                                request.Turnover,
                                FillType.Game,
                                request.RoundId.ToString(),
                                stoppingToken).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing task work item.");
            }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Fill player bucket queued hosted service is stopping.");

        await base.StopAsync(stoppingToken);
    }
}