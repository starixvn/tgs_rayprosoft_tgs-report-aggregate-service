using System.Text;
using System.Text.Json;
using MediatR;
using NATS.Client.Core;
using Tgs.Report.Aggregate.Domain.Models.GameProviderAggregate;
using TGS.Core.Infrastructures.Nats.Interface;

namespace Tgs.Report.Aggregate.Service.MessageHandlers;

public class GameProviderAggregateMessageHandler(
    ILogger<GameProviderAggregateMessageHandler> logger,
    IMediator mediator)
    : IMessageHandler<CreateGameProviderAggregateRequest>
{
    public async Task Handle(string subject, string? queueGroup, NatsHeaders? natsHeaders, CreateGameProviderAggregateRequest? message,
        CancellationToken cancellationToken = default)
    {
        logger.Log(LogLevel.Information,
            new EventId(10000, nameof(GameProviderAggregateMessageHandler)),
            new
            {
                ReceivingOn = DateTimeOffset.UtcNow.ToString("O"),
                message.GameProviderId,
                message.StartDate,
                message.AuditLogId,
                message.RedisKey
            },
            null,
            (state, ex) => JsonSerializer.Serialize(state));

        _ = await mediator.Send(message, cancellationToken).ConfigureAwait(false);
    }
}