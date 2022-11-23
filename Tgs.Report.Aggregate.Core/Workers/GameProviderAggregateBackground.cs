using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tgs.Core.Infrastructures.Utility;
using Tgs.Report.Aggregate.Domain.Interfaces;

namespace Tgs.Report.Aggregate.Core.Workers;

public class GameProviderAggregateBackground : BackgroundService
{
    private readonly ILogger<GameProviderAggregateBackground> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IGameProviderAggregateTaskQueue _taskQueue;

    public GameProviderAggregateBackground(IGameProviderAggregateTaskQueue taskQueue,
        IServiceProvider serviceProvider,
        ILogger<GameProviderAggregateBackground> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Game provider aggregate queued hosted service is running.");

        await BackgroundProcessingAsync(stoppingToken);
    }

    private async Task BackgroundProcessingAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var gameProviderFactory = scope.ServiceProvider.GetRequiredService<IGameProviderAggregateFactory>();
                var redis = scope.ServiceProvider.GetRequiredService<IRedisConnectionFactory>();

                var request =
                    await _taskQueue.DequeueAsync(stoppingToken);

                var db = redis.GetDataBase();

                if (!await db.SetContainsAsync(request.RedisKey, request.GameProviderId.ToString())
                        .ConfigureAwait(false))
                {
                    continue;
                }

                var start = request.StartDate.ToOffset(new(8, 0, 0));
                var end = DateGenerator.GetFirstDayOfMonth(start.AddMonths(1));

                _ = await gameProviderFactory.RemoveGameProviderAggregateAsync(request.GameProviderId, start, end);

                await gameProviderFactory.GenerateGameProviderAggregateAsync(request.GameProviderId,
                    start.ToUniversalTime(), end.ToUniversalTime(), request.AuditLogId).ConfigureAwait(false);

                _ = await db.SetRemoveAsync(request.RedisKey, request.GameProviderId.ToString())
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing task work item.");
            }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Game provider aggregate queued hosted service is stopping.");

        await base.StopAsync(stoppingToken);
    }
}