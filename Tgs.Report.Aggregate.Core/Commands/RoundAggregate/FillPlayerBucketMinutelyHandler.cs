using MediatR;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;
using Tgs.Service.PlayerBuckets;

namespace Tgs.Report.Aggregate.Core.Commands.RoundAggregate;

public class FillPlayerBucketMinutelyHandler : IRequestHandler<FillPlayerBucketMinutelyRequest, MessageResponse>
{
	private readonly IPlayerBucketClient _playerBucketClient;
	private readonly IRoundFactory _roundFactory;

	public FillPlayerBucketMinutelyHandler(
		IPlayerBucketClient playerBucketClient,
		IRoundFactory roundFactory)
	{
		_playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
		_roundFactory = roundFactory ?? throw new ArgumentNullException(nameof(roundFactory));
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

		await Task.WhenAll(models.Select(model => Task.Run(async () => await _playerBucketClient.FillInBucketAsync(
			model.PlayerCode,
			model.Turnover,
			FillType.Game,
			model.RoundId.ToString(),
			cancellationToken).ConfigureAwait(false)))).ConfigureAwait(false);

		return new() { Valid = true };
	}
}
