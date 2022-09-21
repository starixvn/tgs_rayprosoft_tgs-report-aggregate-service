using System.Text;
using System.Text.Json;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tgs.MessageQueue;
using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Shared;

namespace Tgs.Report.Aggregate.Core.Workers;

public class PlayerAggregateBackground : BackgroundService
{
    private readonly ILogger<PlayerAggregateBackground> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IPlayerAggregateTaskQueue _taskQueue;

    public PlayerAggregateBackground(IPlayerAggregateTaskQueue taskQueue,
        IServiceProvider serviceProvider,
        ILogger<PlayerAggregateBackground> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Player Aggregate queued hosted service is running.");

        await BackgroundProcessing(stoppingToken);
    }

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var messageSender = scope.ServiceProvider.GetRequiredService<IMessageSender>();
                var playerAggregateFactory = scope.ServiceProvider.GetRequiredService<IPlayerAggregateFactory>();
                var playerGameAggregateFactory =
                    scope.ServiceProvider.GetRequiredService<IPlayerGameAggregateFactory>();
                var playerAggregateService = scope.ServiceProvider.GetRequiredService<ITgsService<PlayerAggregate>>();
                var playerBonusAggregateService = scope.ServiceProvider.GetRequiredService<ITgsService<PlayerBonusAggregate>>();
                var aggregateExecutionTimeFactory =
                    scope.ServiceProvider.GetRequiredService<IAggregateExecutionTimeFactory>();
                var bonusAggregateFactory = scope.ServiceProvider.GetRequiredService<IPlayerBonusAggregateFactory>();

                var request =
                    await _taskQueue.DequeueAsync(stoppingToken);

                var havePlayerAggregateRecordIsExisted = await playerAggregateService.GetQueries().AnyAsync(x =>
                        x.ReportDate == request.ReportDate.ToUniversalTime() && x.Hour == request.Start.Hour,
                    stoppingToken);

                var playerBonusAggregateRecordIsExisted = await playerBonusAggregateService.GetQueries().AnyAsync(x =>
                        x.ReportDate == request.ReportDate.ToUniversalTime() && x.Hour == request.Start.Hour,
                    stoppingToken);

                if (havePlayerAggregateRecordIsExisted || playerBonusAggregateRecordIsExisted)
                {
                    _logger.LogWarning($"{request.ReportDate.Date} {request.Start.Hour} is already existed.");

                    continue;
                }

                var roundExecutionTimeIsExisted = await aggregateExecutionTimeFactory.ExecutionTimeIsExistsAsync(
                    AggregateType.Round, request.Start,
                    request.End, stoppingToken);

                if (!roundExecutionTimeIsExisted)
                {
                    _logger.LogWarning($"round aggregate is no existed.  {request.ReportDate.Date} {request.Start.Hour}");

                    continue;
                }

                var executionTimeIsExisted = await aggregateExecutionTimeFactory.ExecutionTimeIsExistsAsync(
                    AggregateType.Player, request.Start,
                    request.End, stoppingToken);

                if (executionTimeIsExisted)
                {
                    _logger.LogWarning($"{request.ReportDate.Date} {request.Start.Hour} is already executed.");

                    continue;
                }

                var (aggregates, aggregatesByGameType, aggregateBonus) =
                    await playerAggregateFactory.GetPlayerAggregatesAsync(request.Start, request.End);

                using var transactionScope = new TransactionScope(TransactionScopeOption.Required,
                    new TransactionOptions
                        { Timeout = TimeSpan.FromMinutes(20) }, TransactionScopeAsyncFlowOption.Enabled);

                if (aggregates.Length > 0)
                {
                    await playerAggregateFactory.CreatePlayerAggregatesAsync(aggregates);
                }

                if (aggregatesByGameType.Length > 0)
                {
                    await playerGameAggregateFactory.CreatePlayerGameAggregatesAsync(aggregatesByGameType);
                }

                if (aggregateBonus.Length > 0)
                {
                    await bonusAggregateFactory.CreatePlayerBonusAggregatesAsync(aggregateBonus, stoppingToken);
                }

                _ = await aggregateExecutionTimeFactory.CreateExecutionTimeAsync(AggregateType.Player, request.Start,
                    request.End, stoppingToken);

                transactionScope.Complete();

                var sendForVipLevelUpCalculation = new
                {
                    ReportOn = request.ReportDate,
                    PlayerCodes = aggregates.Select(x => x.PlayerCode).ToArray()
                };

                await messageSender.PublishAsync("tgs.viplevel.aggregate.server",
                        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sendForVipLevelUpCalculation)), stoppingToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing task work item.");
            }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Player aggregate queued hosted service is stopping.");

        await base.StopAsync(stoppingToken);
    }
}