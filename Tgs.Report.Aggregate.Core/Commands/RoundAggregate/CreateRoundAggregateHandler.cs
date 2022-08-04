using System.Transactions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;
using Tgs.Report.Aggregate.Domain.Shared;

namespace Tgs.Report.Aggregate.Core.Commands.RoundAggregate;

public class
	CreateRoundAggregateHandler : IRequestHandler<CreateRoundAggregateRequest,
		MessageResponse>
{
	private readonly IRoundAggregateFactory _roundAggregateFactory;
	private readonly ITgsService<Domain.Entities.RoundAggregate> _tgsRoundAggregateService;
    private readonly IAggregateExecutionTimeFactory _aggregateExecutionTimeFactory;

	public CreateRoundAggregateHandler(
        ITgsService<Domain.Entities.RoundAggregate> tgsRoundAggregateService,
        IRoundAggregateFactory roundAggregateFactory,
        IAggregateExecutionTimeFactory aggregateExecutionTimeFactory)
    {
        _roundAggregateFactory = roundAggregateFactory;
        _tgsRoundAggregateService = tgsRoundAggregateService;
        _aggregateExecutionTimeFactory = aggregateExecutionTimeFactory;
    }

    public async Task<MessageResponse> Handle(CreateRoundAggregateRequest request,
		CancellationToken cancellationToken)
	{
		var previousHour = DateTimeOffset.Now.ToOffset(new(8, 0, 0)).AddHours(-1);
		var theDay = new DateTimeOffset(previousHour.Year, previousHour.Month, previousHour.Day, 0, 0, 0,
			previousHour.Offset);

		var marketsOfDay = await _tgsRoundAggregateService.GetQueries()
			.Where(x => x.ReportDate == theDay.ToUniversalTime())
			.Select(x => x.Hour).Distinct().ToArrayAsync(cancellationToken);

		var hourArray = Enumerable.Range(0, previousHour.Hour + 1);

		var doHours = hourArray.Except(marketsOfDay).ToArray();

		foreach (var doHour in doHours)
		{
			var start = new DateTimeOffset(previousHour.Year, previousHour.Month, previousHour.Day, doHour,
				0, 0, previousHour.Offset);
			var end = new DateTimeOffset(previousHour.Year, previousHour.Month, previousHour.Day, doHour,
				0, 0, previousHour.Offset).AddHours(1);

			if (await _tgsRoundAggregateService.GetQueries().AnyAsync(x =>
					x.ReportDate == theDay.ToUniversalTime() && x.Hour == start.Hour, cancellationToken))
			{
				return new MessageResponse
				{
					Valid = false,
					Message = $"{theDay.Date} {start.Hour} is already existed."
				};
			}

            if (await _aggregateExecutionTimeFactory.ExecutionTimeIsExistsAsync(AggregateType.Round, start, end, cancellationToken))
            {
                continue;
            }

			using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

			_ = await _roundAggregateFactory.GenerateRoundAggregateAsync(start, end);

            _ = await _aggregateExecutionTimeFactory.CreateExecutionTimeAsync(AggregateType.Round, start,
                end, cancellationToken);

			scope.Complete();
        }

		return new() { Valid = true };
	}
}
