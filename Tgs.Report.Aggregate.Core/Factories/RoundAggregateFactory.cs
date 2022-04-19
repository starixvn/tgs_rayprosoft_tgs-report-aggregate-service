using Microsoft.EntityFrameworkCore;
using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;
using Tgs.Report.Aggregate.Domain.Models.PlayerGameAggregate;
using Tgs.Report.Aggregate.Domain.Shared;

namespace Tgs.Report.Aggregate.Core.Factories
{
	public class RoundAggregateFactory : IRoundAggregateFactory
	{
		private readonly ITgsRepository _repository;
		private readonly ITgsService<RoundAggregate> _roundAggregateRepository;
		private readonly ITgsService<Player> _playerRepository;
		private readonly ITgsService<Transaction> _transactionRepository;
		private readonly ITgsService<AdjustPlayerRecord> _adjustPlayerRecordRepository;

		public RoundAggregateFactory(ITgsRepository repository, ITgsService<RoundAggregate> roundAggregateRepository,
			ITgsService<Player> playerRepository, ITgsService<Transaction> transactionRepository,
			ITgsService<AdjustPlayerRecord> adjustPlayerRecordRepository)
		{
			_repository = repository;
			_roundAggregateRepository = roundAggregateRepository;
			_playerRepository = playerRepository;
			_transactionRepository = transactionRepository;
			_adjustPlayerRecordRepository = adjustPlayerRecordRepository;
		}

		public async ValueTask<(PlayerAggregateModel[] playerAggregates, PlayerGameTypeAggregateModel[]
			playerAggregateByGameType)
		> GetPlayerAggregatesAsync(DateTimeOffset start, DateTimeOffset end)
		{
			var reportDate = new DateTimeOffset(start.Year,
				start.Month,
				start.Day,
				0,
				0,
				0,
				start.Offset);

			var utcStart = start.ToUniversalTime();
			var utcEnd = end.ToUniversalTime();

			var roundAggregates =
				await _roundAggregateRepository.GetQueries().Where(x
						=> x.ReportDate == reportDate.ToUniversalTime() && x.Hour == start.Hour &&
						x.Minute == start.Minute)
					.ToArrayAsync();

			var playerAggregates = await _playerRepository.GetQueries().Include(x => x.Agent)
				.GroupJoin(
					_transactionRepository.GetQueries().Where(x => x.RecordedOn >= utcStart && x.RecordedOn < utcEnd)
					,
					p => p.PlayerCode,
					t => t.Wallet.PlayerCode,
					(p, t) => new
					{
						p.BrandId,
						Player = p,
						transaction = t
					})
				.SelectMany(x => x.transaction.DefaultIfEmpty(),
					(x, transaction) => new
					{
						x.BrandId,
						x.Player,
						DividendPercentage = x.Player.Agent == null ? 0M : x.Player.Agent.DividendPercentage,
						transaction
					})
				.GroupJoin(_adjustPlayerRecordRepository.GetQueries().Where(x
						=> x.TransactionRecordedOn >= utcStart &&
						x.TransactionRecordedOn < utcEnd), group => group.transaction!.Id,
					adjust => adjust.TransactionId,
					(group, adjust) => new
					{
						group.BrandId,
						group.Player,
						group.DividendPercentage,
						group.transaction,
						group.transaction!.Wallet,
						group.transaction.Amount,
						adjust
					})
				.SelectMany(x => x.adjust.DefaultIfEmpty(), (x, adjust) => new
				{
					x.BrandId,
					x.Player,
					x.DividendPercentage,
					x.transaction,
					x.Wallet,
					x.Amount,
					AdjustType = adjust == null ? 0 : adjust.Type,
					IncludeInNetProfit = adjust == null ? true : adjust.IncludeInNetProfit
				})
				.GroupBy(x => new
				{
					x.BrandId,
					x.Player.AgentId,
					x.DividendPercentage,
					x.Player.PlayerCode
				},
					(x, players) =>
						new PlayerAggregateModel
						{
							BrandId = x.BrandId,
							Hour = start.Hour,
							ReportDate = reportDate.ToUniversalTime(),
							AgentId = x.AgentId,
							PlayerCode = x.PlayerCode,
							RegisteredCount =
								players.Count(p =>
									p.Player.RegisteredOn >= utcStart && p.Player.RegisteredOn < utcEnd) > 0
									? 1
									: 0,
							FirstFundInCount =
								players.Count(p =>
									p.Player.FirstDepositOn >= utcStart &&
									p.Player.FirstDepositOn < utcEnd) > 0
									? 1
									: 0,
							DepositAmount =
								players.Where(p => p.transaction != null &&
										p.transaction.Type == (short)TransactionType.Deposit)
									.Sum(t => t.transaction == null ? 0M : t.transaction.Amount),
							WithdrawalAmount =
								players.Where(p => p.transaction != null &&
										p.transaction.Type == (short)TransactionType.Withdrawal)
									.Sum(t => t.transaction == null ? 0M : t.transaction.Amount),
							CompanyAdjustAmount = players.Where(p => p.transaction != null &&
									p.transaction.Type == (short)TransactionType.CompanyAdjust)
								.Sum(t => t.transaction == null ? 0M : t.transaction.Amount),
							AgentAdjustAmount = players.Where(p => p.transaction != null &&
									p.transaction.Type == (short)TransactionType.AgentAdjust)
								.Sum(t => t.transaction == null ? 0M : t.transaction.Amount),
							SystemAdjustAmount = players.Where(p => p.transaction != null &&
									p.transaction.Type == (short)TransactionType.SystemAdjust)
								.Sum(t => t.transaction == null ? 0M : t.transaction.Amount),
							BonusAmount = players.Where(p => p.transaction != null &&
								p.transaction.Type == (short)TransactionType.Bonus).Sum(t
								=> t.transaction == null ? 0M : t.transaction.Amount),
							RebateAmount = players.Where(p => p.transaction != null &&
								p.transaction.Type == (short)TransactionType.Rebate).Sum(t
								=> t.transaction == null ? 0M : t.transaction.Amount),
							DividendAmount = players.Where(p => p.transaction != null &&
								p.transaction.Type == (short)TransactionType.Dividend).Sum(t
								=> t.transaction == null ? 0M : t.transaction.Amount),
							PromotionAmount = players.Where(p => p.transaction != null &&
								p.transaction.Type == (short)TransactionType.Promotion).Sum(t
								=> t.transaction == null ? 0M : t.transaction.Amount),
							AllAgentRebateAmount = players.Where(p => p.transaction != null &&
									p.transaction.Type == (short)TransactionType.DividendAllAgentRebate)
								.Sum(t => t.transaction == null ? 0M : t.transaction.Amount),
							NoIncludeNetProfitCompanyAdjustAmount = players.Where(p => p.transaction != null &&
								p.transaction.Type == (short)TransactionType.CompanyAdjust &&
								p.IncludeInNetProfit == false && p.AdjustType ==
								(short)PlayerAdjustmentTransactionType.CompanyAdjust).Sum(t
								=> t.transaction == null ? 0M : t.transaction.Amount),
							NoIncludeNetProfitAgentAdjustAmount = players.Where(p => p.transaction != null &&
								p.transaction.Type == (short)TransactionType.AgentAdjust &&
								p.IncludeInNetProfit == false && p.AdjustType ==
								(short)PlayerAdjustmentTransactionType.AgentAdjust).Sum(t
								=> t.transaction == null ? 0M : t.transaction.Amount),
							CreatedOn = DateTimeOffset.UtcNow
						}).ToArrayAsync();

			foreach (var playerAggregate in playerAggregates)
			{
				var playerRoundAggregates =
					roundAggregates.Where(r => r.PlayerCode == playerAggregate.PlayerCode).ToArray();

				var playerRoundAggregate = playerRoundAggregates.FirstOrDefault();

				if (playerRoundAggregate != null)
				{
					playerAggregate.AgentId = playerRoundAggregate.AgentId;
				}

				playerAggregate.BetAmount =
					playerRoundAggregates
						.Sum(r => r.BetAmount);

				playerAggregate.OriginalTurnover =
					playerRoundAggregates
						.Sum(r => r.OriginalTurnover);

				playerAggregate.ValidBetAmount =
					playerRoundAggregates
						.Sum(r => r.ValidBetAmount);

				playerAggregate.Turnover =
					playerRoundAggregates
						.Sum(r => r.Turnover);

				playerAggregate.WinLossAmount = playerRoundAggregates
					.Sum(r => r.WinLoss);
			}

			var playerAggregateByGameType = roundAggregates.GroupJoin(
					_repository.Players.Include(x => x.Agent).Include(x => x.Vip),
					r => r.PlayerCode,
					p => p.PlayerCode,
					(r, p) => new
					{
						r,
						p = p.DefaultIfEmpty()
					})
				.SelectMany(x => x.p.Select(player => player),
					(x, player) => new
					{
						x.r,
						x.p,
						player?.Vip,
						player?.Agent
					})
				.GroupBy(x => new
				{
					x.r.BrandId,
					x.r.AgentId,
					x.r.PlayerCode,
					x.r.GameProviderId,
					x.r.GameId,
					x.r.GameType,
					x.Vip,
					x.Agent
				},
					(x, aggregate) =>
					{
						return new PlayerGameTypeAggregateModel
						{
							BrandId = x.BrandId,
							AgentId = x.AgentId,
							PlayerCode = x.PlayerCode,
							VipLevel = x.Vip?.Level,
							GameProviderId = x.GameProviderId,
							GameId = x.GameId,
							GameType = (GameType)x.GameType,
							Hour = start.Hour,
							VipSportsRebatePercentage = x.Vip?.SportsRebatePercentage ?? 0M,
							VipVirtualSportsRebatePercentage = x.Vip?.VirtualSportsRebatePercentage ?? 0M,
							VipLiveDealerRebatePercentage = x.Vip?.LiveDealerRebatePercentage ?? 0M,
							VipCardGameRebatePercentage = x.Vip?.CardGameRebatePercentage ?? 0M,
							VipLotteryRebatePercentage = x.Vip?.LotteryRebatePercentage ?? 0M,
							VipSlotRebatePercentage = x.Vip?.SlotRebatePercentage ?? 0M,
							BetAmount = aggregate.Sum(a => a.r.BetAmount),
							ValidBetAmount = aggregate.Sum(a => a.r.ValidBetAmount),
							OriginalTurnover = aggregate.Sum(a => a.r.OriginalTurnover),
							Turnover = aggregate.Sum(a => a.r.Turnover),
							WinLoss = aggregate.Sum(a => a.r.WinLoss),
							VipRebateAmount =
								x.Vip == null
									? 0M
									: GetPlayerRebateAmountByGameType(x.Vip,
										x.GameType,
										aggregate!.Select(a => a.r)),
							CreatedOn = DateTimeOffset.UtcNow,
							ReportDate = reportDate.ToUniversalTime()
						};
					}).ToArray();

			var players = playerAggregateByGameType.Select(x => x.PlayerCode).ToArray();

			playerAggregates = playerAggregates.Where(x =>
				x.RegisteredCount != 0 || x.FirstFundInCount != 0 || x.DepositAmount != 0 || x.WithdrawalAmount != 0 ||
				x.CompanyAdjustAmount != 0 || x.BonusAmount != 0 || x.RebateAmount != 0 || x.DividendAmount != 0 ||
				x.PromotionAmount != 0 || x.BetAmount != 0 || x.OriginalTurnover != 0 || x.ValidBetAmount != 0 ||
				x.Turnover != 0 || x.AgentAdjustAmount != 0 || x.NoIncludeNetProfitAgentAdjustAmount != 0 ||
				x.NoIncludeNetProfitCompanyAdjustAmount != 0 ||
				x.WinLossAmount != 0 || players.Contains(x.PlayerCode)).ToArray();

			return (playerAggregates, playerAggregateByGameType);
		}

		private static decimal GetPlayerRebateAmountByGameType(
			VipLevel vip, short type,
			IEnumerable<RoundAggregate> roundAggregates)
		{
			switch ((GameType)type)
			{
				case GameType.Sportsbook:
				case GameType.ESports:
					return vip.SportsRebatePercentage * roundAggregates.Where(a =>
							a.GameType is (short)GameType.Sportsbook or (short)GameType.ESports)
						.Sum(a => a.Turnover) / 100M;

				case GameType.VirtualSports:
					return vip.VirtualSportsRebatePercentage * roundAggregates.Where(a =>
							a.GameType == (short)GameType.VirtualSports)
						.Sum(a => a.Turnover) / 100M;

				case GameType.LiveDealer:
					return vip.LiveDealerRebatePercentage * roundAggregates.Where(a =>
							a.GameType == (short)GameType.LiveDealer)
						.Sum(a => a.Turnover) / 100M;

				case GameType.CardGame:
					return vip.CardGameRebatePercentage * roundAggregates.Where(a =>
							a.GameType == (short)GameType.CardGame)
						.Sum(a => a.Turnover) / 100M;

				case GameType.Fishing:
				case GameType.Arcade:
				case GameType.Slot:
				case GameType.TableGame:
					return vip.SlotRebatePercentage * roundAggregates.Where(a =>
							a.GameType is (short)GameType.Fishing or (short)GameType.Arcade or (short)GameType.Slot
								or (short)GameType.TableGame)
						.Sum(a => a.Turnover) / 100M;

				case GameType.Lottery:
					return vip.LotteryRebatePercentage * roundAggregates.Where(a =>
							a.GameType == (short)GameType.Lottery)
						.Sum(a => a.Turnover) / 100M;

				default:
					return 0M;
			}
		}
	}
}