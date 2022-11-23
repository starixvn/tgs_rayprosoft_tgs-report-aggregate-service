using MediatR;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;
using Tgs.Service.PlayerBuckets;

namespace Tgs.Report.Aggregate.Core.Commands.RoundAggregate;

public class ManualFillPlayerBucketMinutelyHandler : IRequestHandler<ManualFillPlayerBucketMinutelyRequest, MessageResponse>
{
	private readonly IRoundFactory _roundService;
	private readonly IPlayerBucketClient _playerBucketClient;

	public ManualFillPlayerBucketMinutelyHandler(
		IRoundFactory roundService,
		IPlayerBucketClient playerBucketClient)
	{
		_roundService = roundService ?? throw new ArgumentNullException(nameof(roundService));
		_playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
	}

	public async Task<MessageResponse> Handle(ManualFillPlayerBucketMinutelyRequest request,
		CancellationToken cancellationToken)
	{
		if (!string.IsNullOrEmpty(request.Date)
			&& DateTimeOffset.TryParse($"{request.Date} {request.Hour}:{request.FromMinute} +08:00", out var result)
			&& DateTimeOffset.TryParse($"{request.Date} {request.Hour}:{request.ToMinute} +08:00", out _))
		{
			var start = new DateTimeOffset(result.Year, result.Month, result.Day,
				result.Hour,
				request.FromMinute, 0, result.Offset);

			var end = new DateTimeOffset(result.Year, result.Month, result.Day,
				result.Hour,
				request.ToMinute, 0, result.Offset).AddMinutes(1);

			var models = await _roundService.GetMinuteRoundAggregateAsync(start, end);

			if (models.Length > 0 && request.FillPlayerBucket)
			{
				_ = await Task.WhenAll(models.Select(model => Task.Run(() => _playerBucketClient.FillInBucketAsync(
					model.PlayerCode,
					model.Turnover,
					FillType.Game,
					model.RoundId.ToString(),
					cancellationToken)))).ConfigureAwait(false);
			}

			return new() { Valid = true };
		}

		return new MessageResponse { Valid = false, Message = "Date is invalid." };
	}
}
