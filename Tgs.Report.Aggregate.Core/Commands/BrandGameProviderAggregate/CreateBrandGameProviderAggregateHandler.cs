using MediatR;
using Tgs.Report.Aggregate.Core.Workers;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Models.BrandGameProviderAggregate;

namespace Tgs.Report.Aggregate.Core.Commands.BrandGameProviderAggregate;

public class
    CreateBrandGameProviderAggregateHandler(IBrandGameProviderAggregateTaskQueue taskQueue)
    : IRequestHandler<CreateBrandGameProviderAggregateRequest,
        MessageResponse>
{
    public async Task<MessageResponse> Handle(CreateBrandGameProviderAggregateRequest request,
        CancellationToken cancellationToken)
    {
        await taskQueue.QueueBrandGameProviderAggregateAsync(new BrandGameProviderAggregateQueueModel(request.StartDate,
            request.BrandId,
            request.AuditLogId,
            request.RedisKey));

        return new MessageResponse { Valid = true };
    }
}