using MediatR;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.BrandGameProviderAggregate;

namespace Tgs.Report.Aggregate.Core.Commands.BrandGameProviderAggregate;

public class
    RemoveBrandGameProviderAggregateHandler(IBrandGameProviderAggregateFactory brandGameProviderFactory)
    : IRequestHandler<RemoveBrandGameProviderAggregateRequest,
        MessageResponse>
{
    public async Task<MessageResponse> Handle(RemoveBrandGameProviderAggregateRequest request,
        CancellationToken cancellationToken)
    {
        _ = await brandGameProviderFactory.RemoveBrandGameProviderAggregateAsync(request.BrandId, request.StartDate, request.EndDate);

        return new MessageResponse { Valid = true };
    }
}