using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Aggregate.Core.Workers;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;
using Tgs.Report.Aggregate.Domain.Shared;

namespace Tgs.Report.Aggregate.Core.Commands.PlayerAggregate;

public class
    CreateUpdatePlayerAggregateHandler : IRequestHandler<CreateUpdatePlayerAggregateModel,
        MessageResponse>
{
    private readonly IPlayerAggregateTaskQueue _taskQueue;
    private readonly ITgsService<Domain.Entities.PlayerAggregate> _tgsPlayerAggregateService;

    public CreateUpdatePlayerAggregateHandler(
        IPlayerAggregateTaskQueue taskQueue,
        ITgsService<Domain.Entities.PlayerAggregate> tgsPlayerAggregateService)
    {
        _taskQueue = taskQueue;
        _tgsPlayerAggregateService = tgsPlayerAggregateService;
    }

    public async Task<MessageResponse> Handle(CreateUpdatePlayerAggregateModel request,
        CancellationToken cancellationToken)
    {
        var previousHour = DateTimeOffset.Now.ToOffset(new(8, 0, 0)).AddHours(-1);
        var reportDate = new DateTimeOffset(previousHour.Year,
            previousHour.Month,
            previousHour.Day,
            0,
            0,
            0,
            previousHour.Offset);

        var marketsOfDay = await _tgsPlayerAggregateService.GetQueries()
            .Where(x => x.ReportDate == reportDate.ToUniversalTime())
            .Select(x => x.Hour).Distinct().ToArrayAsync(cancellationToken);

        var hourArray = Enumerable.Range(0, previousHour.Hour + 1);

        var doHours = hourArray.Except(marketsOfDay).ToArray();

        if (doHours.Length == 0)
        {
            return new()
            {
                Valid = false,
                Message = "Not any hours to generate player aggregate."
            };
        }

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

            await _taskQueue.QueuePlayerAggregateAsync(new(reportDate, start, end));
        }

        return new() { Valid = true };
    }
}