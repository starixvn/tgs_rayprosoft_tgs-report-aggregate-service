using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Tgs.Report.Aggregate.Core.Workers;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.BrandGameProviderAggregate;

namespace Tgs.Report.Aggregate.Core.Commands.BrandGameProviderAggregate;

public class
    CreateBrandGameProviderAggregateForScheduleHandler(IBackgroundTaskQueue backgroundTaskQueue)
    : IRequestHandler<CreateBrandGameProviderAggregateForScheduleRequest,
        MessageResponse>
{
    public async Task<MessageResponse> Handle(CreateBrandGameProviderAggregateForScheduleRequest request,
        CancellationToken cancellationToken)
    {
        await backgroundTaskQueue.QueueBackgroundWorkItemAsync(async (scope, token) =>
        {
            var brandGameProviderFactory =
                scope.ServiceProvider.GetRequiredService<IBrandGameProviderAggregateFactory>();

            _ = await brandGameProviderFactory.RemoveBrandGameProviderAggregateAsync(request.BrandId, request.StartDate, request.EndDate);

            await brandGameProviderFactory.GenerateBrandGameProviderAggregateAsync(request.BrandId,
                    request.StartDate,
                    request.EndDate, 
                    0)
                .ConfigureAwait(false);
        });

        return new MessageResponse { Valid = true };
    }
}