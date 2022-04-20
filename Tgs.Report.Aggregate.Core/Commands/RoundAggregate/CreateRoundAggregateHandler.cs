using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

namespace Tgs.Report.Aggregate.Core.Commands.RoundAggregate;

public class
	CreateRoundAggregateHandler : IRequestHandler<CreateRoundAggregateRequest,
		MessageResponse>
{
	private readonly IRoundFactory _roundFactory;
	private readonly ITgsService<Domain.Entities.RoundAggregate> _tgsRoundAggregateService;

	public CreateRoundAggregateHandler(
		ITgsService<Domain.Entities.RoundAggregate> tgsRoundAggregateService,
		IRoundFactory roundFactory)
	{
		_roundFactory = roundFactory;
		_tgsRoundAggregateService = tgsRoundAggregateService;
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

			_ = await _roundFactory.GenerateRoundAggregateAsync(start, end);
		}

		return new() { Valid = true };
	}
}
