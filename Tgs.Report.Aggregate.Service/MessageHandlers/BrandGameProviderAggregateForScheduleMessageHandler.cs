using System.Text.Json;
using MediatR;
using TGS.Core.Infrastructures.Nats.Interface;
using Tgs.Report.Aggregate.Domain.Models.BrandGameProviderAggregate;
using NATS.Client.Core;

namespace Tgs.Report.Aggregate.Service.MessageHandlers;

public class BrandGameProviderAggregateForScheduleMessageHandler(
    ILogger<BrandGameProviderAggregateForScheduleMessageHandler> logger,
    IMediator mediator)
    : IMessageHandler<CreateBrandGameProviderAggregateForScheduleRequest>
{
    public async Task Handle(string subject, string? queueGroup, NatsHeaders? natsHeaders, CreateBrandGameProviderAggregateForScheduleRequest? message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        logger.Log(LogLevel.Information,
            new EventId(10000, nameof(BrandGameProviderAggregateForScheduleMessageHandler)),
            new
            {
                ReceivingOn = DateTimeOffset.UtcNow.ToString("O"),
                message.BrandId,
                message.StartDate,
                message.EndDate
            },
            null,
            (state, ex) => JsonSerializer.Serialize(state));

        _ = await mediator.Send(message, cancellationToken).ConfigureAwait(false);
    }
}