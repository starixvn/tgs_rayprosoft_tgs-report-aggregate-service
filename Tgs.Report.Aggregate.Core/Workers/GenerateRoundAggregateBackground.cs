using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Transactions;
using Tgs.Report.Aggregate.Domain.Interfaces;

namespace Tgs.Report.Aggregate.Core.Workers;

public class GenerateRoundAggregateBackground(
    IGenerateRoundAggregateTaskQueue taskQueue,
    IServiceProvider serviceProvider,
    ILogger<GenerateRoundAggregateBackground> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Generate round aggregate queued hosted service is running.");
        await BackgroundProcessingAsync(stoppingToken);
    }

    private async Task BackgroundProcessingAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested) {
            try
            {
                using var scope = serviceProvider.CreateScope();

                var roundAggregateFactory = scope.ServiceProvider.GetRequiredService<IRoundAggregateFactory>();

                var request = await taskQueue.DequeueAsync(stoppingToken);

                logger.LogInformation($"Processing generate round aggregate {request}");

                using var transactionScope = new TransactionScope(TransactionScopeOption.Required,
                    new TransactionOptions
                        { Timeout = TimeSpan.FromMinutes(30) },
                    TransactionScopeAsyncFlowOption.Enabled);

                await roundAggregateFactory.GenerateRoundAggregateAsync(request.Start, request.End);

                transactionScope.Complete();
            }
            catch (Exception ex) {
                logger.LogError(ex, "Error occurred executing generate round aggregate task work item.");
            }
        }
    }
}
