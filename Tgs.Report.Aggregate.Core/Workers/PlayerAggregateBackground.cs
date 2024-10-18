using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Shared;

namespace Tgs.Report.Aggregate.Core.Workers;

public class PlayerAggregateBackground(
    IPlayerAggregateTaskQueue taskQueue,
    IServiceProvider serviceProvider,
    ILogger<PlayerAggregateBackground> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Player Aggregate queued hosted service is running.");

        await BackgroundProcessingAsync(stoppingToken);
    }

    private async Task BackgroundProcessingAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
            try
            {
                using var scope = serviceProvider.CreateScope();

                var playerAggregateFactory = scope.ServiceProvider.GetRequiredService<IPlayerAggregateFactory>();
                var playerGameAggregateFactory =
                    scope.ServiceProvider.GetRequiredService<IPlayerGameAggregateFactory>();
                var playerAggregateService = scope.ServiceProvider.GetRequiredService<ITgsService<PlayerAggregate>>();
                var playerBonusAggregateService =
                    scope.ServiceProvider.GetRequiredService<ITgsService<PlayerBonusAggregate>>();
                var aggregateExecutionTimeFactory =
                    scope.ServiceProvider.GetRequiredService<IAggregateExecutionTimeFactory>();
                var bonusAggregateFactory = scope.ServiceProvider.GetRequiredService<IPlayerBonusAggregateFactory>();

                var request = await taskQueue.DequeueAsync(stoppingToken);

                var havePlayerAggregateRecordIsExisted = await playerAggregateService.GetQueries()
                    .AnyAsync(x =>
                            x.ReportDate == request.ReportDate.ToUniversalTime() && x.Hour == request.Start.Hour,
                        stoppingToken);

                var playerBonusAggregateRecordIsExisted = await playerBonusAggregateService.GetQueries()
                    .AnyAsync(x =>
                            x.ReportDate == request.ReportDate.ToUniversalTime() && x.Hour == request.Start.Hour,
                        stoppingToken);

                if (havePlayerAggregateRecordIsExisted || playerBonusAggregateRecordIsExisted)
                {
                    logger.LogInformation(
                        "{request.ReportDate.Date} {request.Start.Hour} is already existed.",
                        request.ReportDate.Date,
                        request.Start.Hour);

                    continue;
                }

                var roundExecutionTimeIsExisted = await aggregateExecutionTimeFactory.ExecutionTimeIsExistsAsync(
                    AggregateType.Round,
                    request.Start,
                    request.End,
                    stoppingToken);

                if (!roundExecutionTimeIsExisted)
                {
                    logger.LogWarning(
                        "round aggregate is no existed.  {request.ReportDate.Date} {request.Start.Hour}",
                        request.ReportDate.Date,
                        request.Start.Hour);

                    continue;
                }

                var executionTimeIsExisted = await aggregateExecutionTimeFactory.ExecutionTimeIsExistsAsync(
                    AggregateType.Player,
                    request.Start,
                    request.End,
                    stoppingToken);

                if (executionTimeIsExisted)
                {
                    logger.LogWarning(
                        "{request.ReportDate.Date} {request.Start.Hour} is already executed.",
                        request.ReportDate.Date,
                        request.Start.Hour);

                    continue;
                }

                var (aggregates, aggregatesByGameType, aggregateBonus) =
                    await playerAggregateFactory.GetPlayerAggregatesAsync(request.Start, request.End);

                using var transactionScope = new TransactionScope(TransactionScopeOption.Required,
                    new TransactionOptions
                        { Timeout = TimeSpan.FromMinutes(20) },
                    TransactionScopeAsyncFlowOption.Enabled);

                if (aggregates.Length > 0) await playerAggregateFactory.CreatePlayerAggregatesAsync(aggregates);

                if (aggregatesByGameType.Length > 0)
                    await playerGameAggregateFactory.CreatePlayerGameAggregatesAsync(aggregatesByGameType);

                if (aggregateBonus.Length > 0)
                    _ = await bonusAggregateFactory.CreatePlayerBonusAggregatesAsync(aggregateBonus, stoppingToken);

                _ = await aggregateExecutionTimeFactory.CreateExecutionTimeAsync(AggregateType.Player,
                    request.Start,
                    request.End,
                    stoppingToken);

                transactionScope.Complete();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred executing task work item.");
            }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Player aggregate queued hosted service is stopping.");

        await base.StopAsync(stoppingToken);
    }
}