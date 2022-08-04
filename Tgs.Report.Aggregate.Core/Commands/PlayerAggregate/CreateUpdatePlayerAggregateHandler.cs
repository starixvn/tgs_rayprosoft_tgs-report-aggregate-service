using System.Text;
using System.Text.Json;
using System.Transactions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.MessageQueue;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;
using Tgs.Report.Aggregate.Domain.Shared;

namespace Tgs.Report.Aggregate.Core.Commands.PlayerAggregate;

public class
	CreateUpdatePlayerAggregateHandler : IRequestHandler<CreateUpdatePlayerAggregateModel,
		MessageResponse>
{
	private readonly IMessageSender _messageSender;
	private readonly IPlayerAggregateFactory _playerAggregateFactory;
	private readonly IPlayerGameAggregateFactory _playerGameAggregateFactory;
    private readonly IPlayerBonusAggregateFactory _bonusAggregateFactory;
    private readonly IAggregateExecutionTimeFactory _aggregateExecutionTimeFactory;
	private readonly ITgsService<Domain.Entities.PlayerAggregate> _tgsPlayerAggregateService;

	public CreateUpdatePlayerAggregateHandler(
		ITgsService<Domain.Entities.PlayerAggregate> tgsPlayerAggregateService,
		IMessageSender messageSender,
		IPlayerAggregateFactory playerAggregateFactory,
		IPlayerGameAggregateFactory playerGameAggregateFactory,
        IPlayerBonusAggregateFactory bonusAggregateFactory,
        IAggregateExecutionTimeFactory aggregateExecutionTimeFactory)
	{
		_tgsPlayerAggregateService = tgsPlayerAggregateService;
		_messageSender = messageSender;
		_playerAggregateFactory = playerAggregateFactory;
		_playerGameAggregateFactory = playerGameAggregateFactory;
        _bonusAggregateFactory = bonusAggregateFactory;
		_aggregateExecutionTimeFactory = aggregateExecutionTimeFactory;
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
			return new MessageResponse
			{
				Valid = false,
				Message = "Not any hours to generate player aggregate."
			};
		}

		var playerCodes = Array.Empty<string>();
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

			if (await _tgsPlayerAggregateService.GetQueries().AnyAsync(x =>
					x.ReportDate == reportDate.ToUniversalTime() && x.Hour == start.Hour, cancellationToken))
			{
				return new MessageResponse
				{
					Valid = false,
					Message = $"{reportDate.Date} {start.Hour} is already existed."
				};
			}

			if (!await _aggregateExecutionTimeFactory.ExecutionTimeIsExistsAsync(AggregateType.Round,start,end,cancellationToken) ||
				await _aggregateExecutionTimeFactory.ExecutionTimeIsExistsAsync(AggregateType.Player, start, end, cancellationToken))
			{
				continue;
			}

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

			var (aggregates, aggregatesByGameType,aggregateBonus) =
				await _playerAggregateFactory.GetPlayerAggregatesAsync(start, end);

			if (aggregates.Length > 0)
			{
				await _playerAggregateFactory.CreatePlayerAggregatesAsync(aggregates);
			}

			if (aggregatesByGameType.Length > 0)
			{

				await _playerGameAggregateFactory.CreatePlayerGameAggregatesAsync(aggregatesByGameType);
			}

            if (aggregateBonus.Length > 0)
            {
                await _bonusAggregateFactory.CreatePlayerBonusAggregatesAsync(aggregateBonus, cancellationToken);
            }

			_ = await _aggregateExecutionTimeFactory.CreateExecutionTimeAsync(AggregateType.Player, start,
                end, cancellationToken);

			playerCodes = playerCodes.Union(aggregates.Select(x => x.PlayerCode)).ToArray();

			scope.Complete();
		}

		var sendForVipLevelUpCalculation = new
		{
			ReportOn = reportDate,
			PlayerCodes = playerCodes
		};

		await _messageSender.PublishAsync("tgs.viplevel.aggregate.server",
				Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sendForVipLevelUpCalculation)), cancellationToken)
			.ConfigureAwait(false);

		return new() { Valid = true };
	}
}
