using MediatR;
using Tgs.Core.Infrastructures.Utility;
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
        var start = request.StartDate.ToOffset(new TimeSpan(-4, 0, 0));

        if (start.Day != 1) throw new Exception("The date must be number 1 of each month.");

        var end = DateGenerator.GetFirstDayOfMonth(start.AddMonths(1));

        _ = await brandGameProviderFactory.RemoveBrandGameProviderAggregateAsync(request.BrandId, start, end);

        return new MessageResponse { Valid = true };
    }
}