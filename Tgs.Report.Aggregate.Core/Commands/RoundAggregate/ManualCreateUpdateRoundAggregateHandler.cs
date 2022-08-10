using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;
using Tgs.Report.Aggregate.Domain.Shared;

namespace Tgs.Report.Aggregate.Core.Commands.RoundAggregate;

public class
	ManualCreateUpdateRoundAggregateHandler : IRequestHandler<ManualCreateRoundAggregateModel,
		MessageResponse>
{
	private readonly IRoundAggregateFactory _roundAggregateFactory;
	private readonly ITgsService<Domain.Entities.RoundAggregate> _tgsRoundAggregateService;
    private readonly IAggregateExecutionTimeFactory _aggregateExecutionTimeFactory;

	public ManualCreateUpdateRoundAggregateHandler(
        ITgsService<Domain.Entities.RoundAggregate> tgsRoundAggregateService,
        IRoundAggregateFactory roundAggregateFactory,
        IAggregateExecutionTimeFactory aggregateExecutionTimeFactory)
    {
        _roundAggregateFactory = roundAggregateFactory;
        _tgsRoundAggregateService = tgsRoundAggregateService;
        _aggregateExecutionTimeFactory = aggregateExecutionTimeFactory;
    }

    public async Task<MessageResponse> Handle(ManualCreateRoundAggregateModel request,
		CancellationToken cancellationToken)
	{
		DateTimeOffset reportDate;

		if (!string.IsNullOrEmpty(request.Date) &&
			DateTimeOffset.TryParse($"{request.Date} {request.Hour}:00 +08:00", out var result))
		{
			reportDate = new(result.Year, result.Month, result.Day, 0, 0, 0,
				result.Offset);
		}
		else
		{
			return new MessageResponse { Valid = false, Message = "Date is invalid." };
		}

		var marketsOfDay = await _tgsRoundAggregateService.GetQueries()
			.Where(x => x.ReportDate == reportDate.ToUniversalTime())
			.Select(x => x.Hour).Distinct().ToArrayAsync(cancellationToken);

		var hourArray = Enumerable.Range(0, result.Hour + 1);

		var doHours = hourArray.Except(marketsOfDay).ToArray();

		if (doHours.Length == 0)
		{
			return new MessageResponse { Valid = false, Message = "Not any hours to generate round aggregate." };
		}

		foreach (var doHour in doHours)
		{
			var start = new DateTimeOffset(result.Year, result.Month, result.Day, doHour,
				0, 0, result.Offset);
			var end = new DateTimeOffset(result.Year, result.Month, result.Day, doHour,
				0, 0, result.Offset).AddHours(1);

			if (await _tgsRoundAggregateService.GetQueries().AnyAsync(x =>
					x.ReportDate == reportDate.ToUniversalTime() && x.Hour == start.Hour, cancellationToken) ||
				end > DateTimeOffset.Now ||
                await _aggregateExecutionTimeFactory.ExecutionTimeIsExistsAsync(AggregateType.Round, start, end, cancellationToken))
			{
				continue;
			}

			_ = await _roundAggregateFactory.GenerateRoundAggregateAsync(start, end);

            _ = await _aggregateExecutionTimeFactory.CreateExecutionTimeAsync(AggregateType.Round, start,
                end, cancellationToken);
        }

		return new() { Valid = true };
	}
}
