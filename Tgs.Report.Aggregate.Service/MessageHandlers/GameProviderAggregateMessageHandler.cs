using System.Text;
using System.Text.Json;
using MediatR;
using Tgs.MessageQueue;
using Tgs.Report.Aggregate.Domain.Models.GameProviderAggregate;

namespace Tgs.Report.Aggregate.Service.MessageHandlers
{
    public class GameProviderAggregateMessageHandler : IMessageHandler
    {
        private readonly ILogger<GameProviderAggregateMessageHandler> _logger;
        private readonly IMediator _mediator;

        public GameProviderAggregateMessageHandler(
            ILogger<GameProviderAggregateMessageHandler> logger,
            IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            var json = Encoding.UTF8.GetString(data.Span);

            var request = JsonSerializer.Deserialize<CreateGameProviderAggregateRequest>(json)!;

            _logger.Log(LogLevel.Information,
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

            await _mediator.Send(request, cancellationToken).ConfigureAwait(false);
        }
    }
}