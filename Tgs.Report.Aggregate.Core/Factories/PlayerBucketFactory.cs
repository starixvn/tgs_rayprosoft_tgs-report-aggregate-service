using System.Text.Json;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models;

namespace Tgs.Report.Aggregate.Core.Factories;

public class PlayerBucketFactory : IPlayerBucketFactory
{
	private readonly ITgsService<PlayerBucketHistory> _playerBucketHistoryService;
	private readonly ITgsService<PlayerBucket> _playerBucketService;

	public PlayerBucketFactory(ITgsService<PlayerBucket> playerBucketService,
		ITgsService<PlayerBucketHistory> playerBucketHistoryService)
	{
		_playerBucketService = playerBucketService;
		_playerBucketHistoryService = playerBucketHistoryService;
	}

	public async ValueTask FillInBucketAsync(DateTimeOffset start, DateTimeOffset end, RoundMinutelyModel[] models)
	{
		var playerCodes = models.GroupBy(x => x.PlayerCode, (playerCode, list) => playerCode).ToArray();

		var playerBuckets = await _playerBucketService.GetQueries()
			.Where(x => playerCodes.Contains(x.PlayerCode) && !x.IsFull).ToArrayAsync().ConfigureAwait(false);

		var playerBucketHistories = new List<PlayerBucketHistory>();

		foreach (var playerCode in playerCodes)
		{
			var roundMinutelyRecords = models.Where(x => x.PlayerCode == playerCode).Select(x
				=> new RoundMinutelyRecordModel
				{
					RoundId = x.RoundId,
					PlayerCode = x.PlayerCode,
					OriginalTurnover = x.Turnover,
					Turnover = x.Turnover * x.EffectiveBettingPercentage / 100M,
					EffectiveBettingPercentage = x.EffectiveBettingPercentage,
					RemainingTurnover = x.Turnover * x.EffectiveBettingPercentage / 100M
				}).ToArray();

			var buckets = playerBuckets.Where(x => x.PlayerCode == playerCode).ToArray();

			foreach (var playerBucket in buckets)
			{
				var roundUsedRecord = new List<RoundUsedRecord>();
				FillBucketByEachRound(playerBucket, roundUsedRecord, roundMinutelyRecords);

				playerBucketHistories.Add(new()
				{
					PlayerBucketId = playerBucket.Id,
					PlayerCode = playerCode,
					Turnover = roundUsedRecord.Sum(x => x.UsedTurnover),
					From = start.ToUniversalTime(),
					To = end.ToUniversalTime(),
					CreatedOn = DateTimeOffset.UtcNow,
					RoundData = JsonSerializer.Serialize(roundUsedRecord)
				});
			}
		}

		using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

		await _playerBucketService.UpdateEntitiesAsync(playerBuckets).ConfigureAwait(false);

		await _playerBucketHistoryService.CreateEntitiesAsync(playerBucketHistories.ToArray())
			.ConfigureAwait(false);

		scope.Complete();
	}

	private static (decimal bucketAmount, decimal currentBucketAmount, decimal usedTurnover) FillBucket(
		decimal bucketAmount, decimal currentBucketAmount, decimal turnover)
	{
		if (turnover <= 0M)
		{
			return (bucketAmount, currentBucketAmount, 0M);
		}

		decimal usedTurnover;

		if (turnover - (bucketAmount - currentBucketAmount) >= 0.0M)
		{
			usedTurnover = bucketAmount - currentBucketAmount;
			currentBucketAmount = bucketAmount;
		}
		else
		{
			usedTurnover = turnover;
			currentBucketAmount += turnover;
		}

		return (bucketAmount, currentBucketAmount, usedTurnover);
	}

	private static void FillBucketByEachRound(PlayerBucket playerBucket,
		ICollection<RoundUsedRecord> roundUsedRecord,
		IEnumerable<RoundMinutelyRecordModel> roundMinutelyRecords)
	{
		foreach (var round in roundMinutelyRecords.Where(x => x.RemainingTurnover > 0))
		{
			var turnover = round.RemainingTurnover;
			var (bucketAmount, currentBucketAmount, usedTurnover) =
				FillBucket(playerBucket.BucketAmount, playerBucket.CurrentBucketAmount, turnover);

			roundUsedRecord.Add(new RoundUsedRecord
			{
				RoundId = round.RoundId,
				PlayerCode = round.PlayerCode,
				OriginalTurnover = round.OriginalTurnover,
				Turnover = round.Turnover,
				EffectiveBettingPercentage = round.EffectiveBettingPercentage,
				UsedTurnover = usedTurnover
			});

			round.RemainingTurnover = turnover - usedTurnover;

			if (bucketAmount == currentBucketAmount)
			{
				playerBucket.CurrentBucketAmount = bucketAmount;
				playerBucket.IsFull = true;
				break;
			}

			playerBucket.CurrentBucketAmount += usedTurnover;
		}
	}
}
