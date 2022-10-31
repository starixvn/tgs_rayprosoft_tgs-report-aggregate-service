using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tgs.Core.Infrastructures.Utility;
using Tgs.Report.Aggregate.Domain.Interfaces;

namespace Tgs.Report.Aggregate.Core.Workers;

public class BrandGameProviderAggregateBackground : BackgroundService
{
    private readonly ILogger<BrandGameProviderAggregateBackground> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IBrandGameProviderAggregateTaskQueue _taskQueue;

    public BrandGameProviderAggregateBackground(IBrandGameProviderAggregateTaskQueue taskQueue,
        IServiceProvider serviceProvider,
        ILogger<BrandGameProviderAggregateBackground> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Brand game provider aggregate queued hosted service is running.");

        await BackgroundProcessing(stoppingToken);
    }

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var brandGameProviderFactory = scope.ServiceProvider.GetRequiredService<IBrandGameProviderAggregateFactory>();
                var redis = scope.ServiceProvider.GetRequiredService<IRedisConnectionFactory>();

                var request =
                    await _taskQueue.DequeueAsync(stoppingToken);

                var db = redis.GetDataBase();

                if (!await db.SetContainsAsync(request.RedisKey, request.BrandId.ToString())
                        .ConfigureAwait(false))
                {
                    continue;
                }

                var start = request.StartDate.ToOffset(new(8, 0, 0));
                var end = DateGenerator.GetFirstDayOfMonth(start.AddMonths(1));

                _ = await brandGameProviderFactory.RemoveBrandGameProviderAggregateAsync(request.BrandId, start, end);

                await brandGameProviderFactory.GenerateBrandGameProviderAggregateAsync(request.BrandId,
                    start.ToUniversalTime(), end.ToUniversalTime(), request.AuditLogId).ConfigureAwait(false);

                _ = await db.SetRemoveAsync(request.RedisKey, request.BrandId.ToString())
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing task work item.");
            }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Brand game provider aggregate queued hosted service is stopping.");

        await base.StopAsync(stoppingToken);
    }
}