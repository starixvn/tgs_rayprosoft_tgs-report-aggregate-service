using MediatR;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

namespace Tgs.Report.Aggregate.Core.Commands.RoundAggregate;

public class
	FillPlayerBucketMinutelyHandler : IRequestHandler<FillPlayerBucketMinutelyRequest,
		MessageResponse>
{
	private readonly IPlayerBucketFactory _playerBucketFactory;
	private readonly IRoundFactory _roundFactory;

	public FillPlayerBucketMinutelyHandler(IPlayerBucketFactory playerBucketFactory,
		IRoundFactory roundFactory)
	{
		_roundFactory = roundFactory;
		_playerBucketFactory = playerBucketFactory;
	}

	public async Task<MessageResponse> Handle(FillPlayerBucketMinutelyRequest request,
		CancellationToken cancellationToken)
	{
		var previousMinute = request.StartDate.ToOffset(new(8, 0, 0));

		var start = new DateTimeOffset(previousMinute.Year, previousMinute.Month, previousMinute.Day,
			previousMinute.Hour,
			previousMinute.Minute, 0, previousMinute.Offset);

		var end = new DateTimeOffset(previousMinute.Year, previousMinute.Month, previousMinute.Day,
			previousMinute.Hour,
			previousMinute.Minute, 0, previousMinute.Offset).AddMinutes(1);

		var models = await _roundFactory.GetMinuteRoundAggregateAsync(start, end);

		if (models.Length > 0)
		{
			await _playerBucketFactory.FillInBucketAsync(start, end, models);
		}

		return new() { Valid = true };
	}
}
