using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Transactions;
using Tgs.Report.Aggregate.Domain.Interfaces;

namespace Tgs.Report.Aggregate.Core.Workers;

public class GeneratePlayerAggregateBackground(
    IGeneratePlayerAggregateTaskQueue taskQueue,
    IServiceProvider serviceProvider,
    ILogger<GeneratePlayerAggregateBackground> logger) 
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Generate player aggregate queued hosted service is running.");
        await BackgroundProcessingAsync(stoppingToken);
    }

    private async Task BackgroundProcessingAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested) {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var playerAggregateFactory = scope.ServiceProvider.GetRequiredService<IPlayerAggregateFactory>();

                var request = await taskQueue.DequeueAsync(stoppingToken);

                Console.WriteLine(request);

                var (aggregates, aggregatesByGameType, aggregateBonus) =
                  await playerAggregateFactory.GetPlayerAggregatesAsync(request.Start, request.End);

                using var transactionScope = new TransactionScope(TransactionScopeOption.Required,
                  new TransactionOptions
                  { Timeout = TimeSpan.FromMinutes(30) },
                TransactionScopeAsyncFlowOption.Enabled);

                await playerAggregateFactory.RemovePlayerAggregatesAsync(request.Start, request.End);
                if (aggregates.Length > 0)
                {
                    await playerAggregateFactory.CreatePlayerAggregatesAsync(aggregates);
                }

                transactionScope.Complete();

            }
            catch (Exception ex) {
                logger.LogError(ex, "Error occurred executing generate player aggregate task work item.");
            }
        }
    }
}
