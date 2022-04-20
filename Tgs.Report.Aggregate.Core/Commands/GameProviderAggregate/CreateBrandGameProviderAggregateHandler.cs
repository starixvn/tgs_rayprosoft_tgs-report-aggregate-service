using MediatR;
using Tgs.Core.Infrastructures.Utility;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.GameProviderAggregate;

namespace Tgs.Report.Aggregate.Core.Commands.GameProviderAggregate;

public class
	CreateGameProviderAggregateHandler : IRequestHandler<CreateGameProviderAggregateRequest,
		MessageResponse>
{
	private readonly IGameProviderAggregateFactory _gameProviderFactory;
	private readonly IRedisConnectionFactory _redisConnectionFactory;

	public CreateGameProviderAggregateHandler(IGameProviderAggregateFactory gameProviderFactory,
		IRedisConnectionFactory redisConnectionFactory)
	{
		_gameProviderFactory = gameProviderFactory;
		_redisConnectionFactory = redisConnectionFactory;
	}

	public async Task<MessageResponse> Handle(CreateGameProviderAggregateRequest request,
		CancellationToken cancellationToken)
	{
		var db = _redisConnectionFactory.GetDataBase();

		if (await db.SetContainsAsync(request.RedisKey, request.GameProviderId.ToString())
				.ConfigureAwait(false))
		{
			var start = request.StartDate.ToOffset(new(8, 0, 0));
			var end = DateGenerator.GetFirstDayOfMonth(start.AddMonths(1));

			await _gameProviderFactory.GenerateGameProviderAggregateAsync(request.GameProviderId,
				start.ToUniversalTime(), end.ToUniversalTime(), request.AuditLogId).ConfigureAwait(false);

			_ = await db.SetRemoveAsync(request.RedisKey, request.GameProviderId.ToString()).ConfigureAwait(false);
		}

		return new() { Valid = true };
	}
}
