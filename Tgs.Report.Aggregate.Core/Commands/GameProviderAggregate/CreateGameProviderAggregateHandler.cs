using MediatR;
using Tgs.Report.Aggregate.Core.Workers;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Models.GameProviderAggregate;

namespace Tgs.Report.Aggregate.Core.Commands.GameProviderAggregate;

public class
    CreateGameProviderAggregateHandler : IRequestHandler<CreateGameProviderAggregateRequest,
        MessageResponse>
{
    private readonly IGameProviderAggregateTaskQueue _taskQueue;

    public CreateGameProviderAggregateHandler(IGameProviderAggregateTaskQueue taskQueue)
    {
        _taskQueue = taskQueue;
    }

    public async Task<MessageResponse> Handle(CreateGameProviderAggregateRequest request,
        CancellationToken cancellationToken)
    {
        await _taskQueue.QueueGameProviderAggregateAsync(new(request.StartDate,
            request.GameProviderId, request.AuditLogId, request.RedisKey));

        return new() { Valid = true };
    }
}