using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Aggregate.Core.Workers;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

namespace Tgs.Report.Aggregate.Core.Commands.PlayerAggregate;

public class GeneratePlayerAggregateHandler(
    ITgsService<Domain.Entities.RoundAggregate> tgsRoundAggregateService,
    IGeneratePlayerAggregateTaskQueue taskQueue) 
    : IRequestHandler<GeneratePlayerAggregateRequest, MessageResponse>
{
    public async Task<MessageResponse> Handle(GeneratePlayerAggregateRequest request, CancellationToken cancellationToken)
    {
        var currentDate = DateTimeOffset.UtcNow.AddDays(-1);
        var startRangeDate = currentDate.AddDays(-14);

        var marketsOfDay = (await tgsRoundAggregateService.GetQueries()
            .Where(x => x.ReportDate >= startRangeDate)
            .Where(x => x.ReportDate < currentDate)
            .Select(x => x.ReportDate)
            .Distinct()
            .ToArrayAsync(cancellationToken))
            .OrderBy(x => x.ToUniversalTime());

        foreach (var date in marketsOfDay)
        {
            var hourArray = Enumerable.Range(0, 24);

            foreach (var hour in hourArray)
            {
                var startTimeWithOffset = new DateTimeOffset(date).ToOffset(new TimeSpan(-4, 0, 0));
                var start = new DateTimeOffset(startTimeWithOffset.Year,
                    startTimeWithOffset.Month,
                    startTimeWithOffset.Day,
                    hour,
                    0,
                    0,
                    startTimeWithOffset.Offset);

                var end = new DateTimeOffset(startTimeWithOffset.Year,
                    startTimeWithOffset.Month,
                    startTimeWithOffset.Day,
                    hour,
                    0,
                    0,
                    startTimeWithOffset.Offset).AddHours(1);

                await taskQueue.QueueGeneratePlayerAggregateAsync(
                   new GeneratePlayerAggregateQueueModel(start, end));
            }
        }

        return new MessageResponse { Valid = true }; 
    }
}
