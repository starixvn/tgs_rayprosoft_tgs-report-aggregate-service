using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Aggregate.Core.Workers;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

namespace Tgs.Report.Aggregate.Core.Commands.PlayerAggregate;

public class
    CreateUpdatePlayerAggregateHandler(
        IPlayerAggregateTaskQueue taskQueue,
        ITgsService<Domain.Entities.PlayerAggregate> tgsPlayerAggregateService)
    : IRequestHandler<CreateUpdatePlayerAggregateModel,
        MessageResponse>
{
    public async Task<MessageResponse> Handle(CreateUpdatePlayerAggregateModel request,
        CancellationToken cancellationToken)
    {
        var previousHour = DateTimeOffset.Now.ToOffset(new TimeSpan(-4, 0, 0)).AddHours(-1);
        var reportDate = new DateTimeOffset(previousHour.Year,
            previousHour.Month,
            previousHour.Day,
            0,
            0,
            0,
            previousHour.Offset);

        var marketsOfDay = await tgsPlayerAggregateService.GetQueries()
            .Where(x => x.ReportDate == reportDate.ToUniversalTime())
            .Select(x => x.Hour)
            .Distinct()
            .ToArrayAsync(cancellationToken);

        var hourArray = Enumerable.Range(0, previousHour.Hour + 1);

        var doHours = hourArray.Except(marketsOfDay).ToArray();

        if (doHours.Length == 0)
            return new MessageResponse
            {
                Valid = false,
                Message = "Not any hours to generate player aggregate."
            };

        foreach (var doHour in doHours)
        {
            var start = new DateTimeOffset(previousHour.Year,
                previousHour.Month,
                previousHour.Day,
                doHour,
                0,
                0,
                previousHour.Offset);
            var end = new DateTimeOffset(previousHour.Year,
                previousHour.Month,
                previousHour.Day,
                doHour,
                0,
                0,
                previousHour.Offset).AddHours(1);

            await taskQueue.QueuePlayerAggregateAsync(new PlayerAggregateQueueModel(reportDate, start, end));
        }

        return new MessageResponse { Valid = true };
    }
}