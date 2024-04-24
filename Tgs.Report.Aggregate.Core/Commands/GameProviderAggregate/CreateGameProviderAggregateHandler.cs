using MediatR;
using Tgs.Report.Aggregate.Core.Workers;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Models.GameProviderAggregate;

namespace Tgs.Report.Aggregate.Core.Commands.GameProviderAggregate;

public class
    CreateGameProviderAggregateHandler(IGameProviderAggregateTaskQueue taskQueue)
    : IRequestHandler<CreateGameProviderAggregateRequest,
        MessageResponse>
{
    public async Task<MessageResponse> Handle(CreateGameProviderAggregateRequest request,
        CancellationToken cancellationToken)
    {
        await taskQueue.QueueGameProviderAggregateAsync(new GameProviderAggregateQueueModel(request.StartDate,
            request.GameProviderId,
            request.AuditLogId,
            request.RedisKey));

        return new MessageResponse { Valid = true };
    }
}