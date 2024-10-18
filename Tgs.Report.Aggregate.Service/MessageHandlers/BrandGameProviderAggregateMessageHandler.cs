using System.Text.Json;
using MediatR;
using NATS.Client.Core;
using Tgs.Report.Aggregate.Domain.Models.BrandGameProviderAggregate;
using TGS.Core.Infrastructures.Nats.Interface;

namespace Tgs.Report.Aggregate.Service.MessageHandlers;

public class BrandGameProviderAggregateMessageHandler(
    ILogger<BrandGameProviderAggregateMessageHandler> logger,
    IMediator mediator)
    : IMessageHandler<CreateBrandGameProviderAggregateRequest>
{
    public async Task Handle(string subject, string? queueGroup, NatsHeaders? natsHeaders, CreateBrandGameProviderAggregateRequest? message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        logger.Log(LogLevel.Information,
            new EventId(10000, nameof(BrandGameProviderAggregateMessageHandler)),
            new
            {
                ReceivingOn = DateTimeOffset.UtcNow.ToString("O"),
                message.BrandId,
                message.StartDate,
                message.AuditLogId,
                message.RedisKey
            },
            null,
            (state, ex) => JsonSerializer.Serialize(state));

        _ = await mediator.Send(message, cancellationToken).ConfigureAwait(false);
    }
}