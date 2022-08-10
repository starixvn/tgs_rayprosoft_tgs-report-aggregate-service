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
                var roundAggregateFactory = scope.ServiceProvider.GetRequiredService<IRoundAggregateFactory>();
                var playerAggregateService = scope.ServiceProvider.GetRequiredService<ITgsService<PlayerAggregate>>();
                var roundAggregateService = scope.ServiceProvider.GetRequiredService<ITgsService<RoundAggregate>>();
                var roundService = scope.ServiceProvider.GetRequiredService<ITgsService<Round>>();

                var request =
                    await _taskQueue.DequeueAsync(stoppingToken);

                if (await playerAggregateService.GetQueries().AnyAsync(x =>
                            x.ReportDate == request.ReportDate.ToUniversalTime() && x.Hour == request.Start.Hour,
                        stoppingToken))
                {
                    _logger.LogWarning($"{request.ReportDate.Date} {request.Start.Hour} is already existed.");

                    return;
                }

                if (!await roundAggregateService.GetQueries().AnyAsync(x =>
                            x.ReportDate == request.ReportDate.ToUniversalTime() && x.Hour == request.Start.Hour,
                        stoppingToken) &&
                    await roundService.GetQueries().AnyAsync(x =>
                            x.ClosedOn >= request.Start.ToUniversalTime() &&
                            x.ClosedOn >= request.End.ToUniversalTime(),
                        stoppingToken))
                {
                    continue;
                }

                var (aggregates, aggregatesByGameType) =
                    await roundAggregateFactory.GetPlayerAggregatesAsync(request.Start, request.End);

                using var transactionScope = new TransactionScope(TransactionScopeOption.Required,
                    new TransactionOptions
                        { Timeout = TimeSpan.FromMinutes(5) }, TransactionScopeAsyncFlowOption.Enabled);

                if (aggregates.Length > 0)
                {
                    await playerAggregateFactory.CreatePlayerAggregatesAsync(aggregates);
                }

                if (aggregatesByGameType.Length > 0)
                {
                    await playerGameAggregateFactory.CreatePlayerGameAggregatesAsync(aggregatesByGameType);
                }

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