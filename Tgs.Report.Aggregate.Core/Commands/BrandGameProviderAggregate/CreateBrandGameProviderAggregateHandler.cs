using MediatR;
using Tgs.Report.Aggregate.Core.Workers;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Models.BrandGameProviderAggregate;

namespace Tgs.Report.Aggregate.Core.Commands.BrandGameProviderAggregate;

public class
    CreateBrandGameProviderAggregateHandler : IRequestHandler<CreateBrandGameProviderAggregateRequest,
        MessageResponse>
{
    private readonly IBrandGameProviderAggregateTaskQueue _taskQueue;

    public CreateBrandGameProviderAggregateHandler(IBrandGameProviderAggregateTaskQueue taskQueue)
    {
        _taskQueue = taskQueue;
    }

    public async Task<MessageResponse> Handle(CreateBrandGameProviderAggregateRequest request,
        CancellationToken cancellationToken)
    {
        await _taskQueue.QueueBrandGameProviderAggregateAsync(new(request.StartDate,
            request.BrandId, request.AuditLogId, request.RedisKey));

        return new() { Valid = true };
    }
}