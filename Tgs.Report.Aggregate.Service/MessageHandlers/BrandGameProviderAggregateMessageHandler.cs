using System.Text;
using System.Text.Json;
using MediatR;
using Tgs.Report.Aggregate.Domain.Models.BrandGameProviderAggregate;
using Valhalla.MessageQueue;

namespace Tgs.Report.Aggregate.Service.MessageHandlers;

public class BrandGameProviderAggregateMessageHandler(
    ILogger<BrandGameProviderAggregateMessageHandler> logger,
    IMediator mediator)
    : IMessageHandler
{
    public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);

        var request = JsonSerializer.Deserialize<CreateBrandGameProviderAggregateRequest>(json)!;

        logger.Log(LogLevel.Information,
            new EventId(10000, nameof(BrandGameProviderAggregateMessageHandler)),
            new
            {
                ReceivingOn = DateTimeOffset.UtcNow.ToString("O"),
                request.BrandId,
                request.StartDate,
                request.AuditLogId,
                request.RedisKey
            },
            null,
            (state, ex) => JsonSerializer.Serialize(state));

        _ = await mediator.Send(request, cancellationToken).ConfigureAwait(false);
    }
}