using System.Text;
using System.Text.Json;
using MediatR;
using Tgs.Report.Aggregate.Domain.Models.GameProviderAggregate;
using Valhalla.MessageQueue;

namespace Tgs.Report.Aggregate.Service.MessageHandlers;

public class GameProviderAggregateMessageHandler(
    ILogger<GameProviderAggregateMessageHandler> logger,
    IMediator mediator)
    : IMessageHandler
{
    public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);

        var request = JsonSerializer.Deserialize<CreateGameProviderAggregateRequest>(json)!;

        logger.Log(LogLevel.Information,
            new EventId(10000, nameof(GameProviderAggregateMessageHandler)),
            new
            {
                ReceivingOn = DateTimeOffset.UtcNow.ToString("O"),
                request.GameProviderId,
                request.StartDate,
                request.AuditLogId,
                request.RedisKey
            },
            null,
            (state, ex) => JsonSerializer.Serialize(state));

        _ = await mediator.Send(request, cancellationToken).ConfigureAwait(false);
    }
}