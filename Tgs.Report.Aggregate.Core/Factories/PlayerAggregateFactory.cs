using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Tgs.Report.Aggregate.Core.Extensions;
using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;
using Tgs.Report.Aggregate.Domain.Models.PlayerGameAggregate;
using Tgs.Report.Aggregate.Domain.Shared;

namespace Tgs.Report.Aggregate.Core.Factories;

public class PlayerAggregateFactory : IPlayerAggregateFactory
{
    private readonly ITgsService<AdjustPlayerRecord> _adjustPlayerRecordRepository;
    private readonly ITgsService<Player> _playerRepository;
    private readonly ITgsRepository _repository;
    private readonly ITgsService<RoundAggregate> _roundAggregateRepository;
    private readonly ITgsService<Transaction> _transactionRepository;

    public PlayerAggregateFactory(ITgsRepository repository, ITgsService<RoundAggregate> roundAggregateRepository,
        ITgsService<Player> playerRepository, ITgsService<Transaction> transactionRepository,
        ITgsService<AdjustPlayerRecord> adjustPlayerRecordRepository)
    {
        _repository = repository;
        _roundAggregateRepository = roundAggregateRepository;
        _playerRepository = playerRepository;
        _transactionRepository = transactionRepository;
        _adjustPlayerRecordRepository = adjustPlayerRecordRepository;
    }

    public Task CreatePlayerAggregatesAsync(PlayerAggregateModel[] models)
    {
        _ = _repository.PlayerAggregates.AddRangeAsync(models.Select(model => new PlayerAggregate
        {
            BrandId = model.BrandId,
            AgentId = model.AgentId,
            PlayerCode = model.PlayerCode,
            Hour = model.Hour,
            ReportDate = model.ReportDate,
            CreatedOn = DateTimeOffset.UtcNow,
            RegisteredCount = model.RegisteredCount,
            FirstFundInCount = model.FirstFundInCount,
            DepositAmount = model.DepositAmount.Truncate(2),
            WithdrawalAmount = model.WithdrawalAmount.Truncate(2),
            CompanyAdjustAmount = model.CompanyAdjustAmount.Truncate(2),
            AgentAdjustAmount = model.AgentAdjustAmount.Truncate(2),
            SystemAdjustAmount = model.SystemAdjustAmount.Truncate(2),
            BonusAdjustAmount = model.BonusAdjustAmount.Truncate(2),
            BonusAmount = model.BonusAmount.Truncate(2),
            RebateAmount = model.RebateAmount.Truncate(2),
            PromotionAmount = model.PromotionAmount.Truncate(2),
            DividendAmount = model.DividendAmount.Truncate(2),
            BetAmount = model.BetAmount.Truncate(2),
            ValidBetAmount = model.ValidBetAmount.Truncate(2),
            OriginalTurnover = model.OriginalTurnover.Truncate(2),
            Turnover = model.Turnover.Truncate(2),
            WinLossAmount = model.WinLossAmount.Truncate(2),
            AllAgentRebateAmount = model.AllAgentRebateAmount.Truncate(2),
            NoIncludeNetProfitCompanyAdjustAmount = model.NoIncludeNetProfitCompanyAdjustAmount.Truncate(2),
            NoIncludeNetProfitAgentAdjustAmount = model.NoIncludeNetProfitAgentAdjustAmount.Truncate(2),
            NoIncludeNetProfitBonusAdjustAmount = model.NoIncludeNetProfitBonusAdjustAmount.Truncate(2),
            AgentDividendPercentage = model.AgentDividendPercentage,
            ChiefAgentDividendPercentage = model.ChiefAgentDividendPercentage,
            AgentDividendPercentageTreeData = model.AgentDividendPercentageTreeData
        }));

        return _repository.SaveChangesAsync();
    }

    public async
        ValueTask<(PlayerAggregateModel[] playerAggregate, PlayerGameTypeAggregateModel[] playerAggregateByGameType, PlayerBonusAggregateModel[] playerBonusAggregate)>
        GetPlayerAggregatesAsync(
            DateTimeOffset start,
            DateTimeOffset end)
    {
        var reportDate = new DateTimeOffset(
            start.Year,
            start.Month,
            start.Day,
            0,
            0,
            0,
            start.Offset);

        var utcStart = start.ToUniversalTime();
        var utcEnd = end.ToUniversalTime();

        var roundAggregates = await _roundAggregateRepository.GetQueries()
            .Where(x => x.ReportDate == reportDate.ToUniversalTime()
                && x.Hour == start.Hour
                && x.Minute == start.Minute)
            .Join(
                _repository.Players.Include(x => x.Vip),
                r => r.PlayerCode,
                p => p.PlayerCode,
                (r, p) => new
                {
                    r,
                    p.Vip
                })
            .Select(x => new PlayerGameTypeAggregateModel
            {
                BrandId = x.r.BrandId,
                AgentId = x.r.AgentId,
                PlayerCode = x.r.PlayerCode,
                VipLevel = x.Vip == null ? null : x.Vip.Level,
                GameProviderId = x.r.GameProviderId,
                GameId = x.r.GameId,
                GameType = (GameType)x.r.GameType,
                Hour = start.Hour,
                VipSportsRebatePercentage = x.Vip == null ? 0M : x.Vip.SportsRebatePercentage,
                VipVirtualSportsRebatePercentage = x.Vip == null ? 0M : x.Vip.VirtualSportsRebatePercentage,
                VipLiveDealerRebatePercentage = x.Vip == null ? 0M : x.Vip.LiveDealerRebatePercentage,
                VipCardGameRebatePercentage = x.Vip == null ? 0M : x.Vip.CardGameRebatePercentage,
                VipLotteryRebatePercentage = x.Vip == null ? 0M : x.Vip.LotteryRebatePercentage,
                VipSlotRebatePercentage = x.Vip == null ? 0M : x.Vip.SlotRebatePercentage,
                BetAmount = x.r.BetAmount,
                OriginalTurnover = x.r.OriginalTurnover,
                Turnover = x.r.Turnover,
                ValidBetAmount = x.r.ValidBetAmount,
                WinLoss = x.r.WinLoss,
                CreatedOn = DateTimeOffset.UtcNow,
                ReportDate = reportDate.ToUniversalTime()
            })
            .ToArrayAsync();

        var playerTotals = await _playerRepository.GetQueries().Include(x => x.Agent)
            .GroupJoin(
                _transactionRepository.GetQueries().Where(x => x.RecordedOn >= utcStart && x.RecordedOn < utcEnd),
                p => p.PlayerCode,
                t => t.Wallet.PlayerCode,
                (p, t) => new
                {
                    p,
                    transaction = t
                })
            .SelectMany(
                x => x.transaction.DefaultIfEmpty(),
                (x, transaction) => new
                {
                    x.p,
                    transaction
                })
            .GroupJoin(
                _adjustPlayerRecordRepository.GetQueries()
                    .Where(x => x.CreatedOn >= utcStart),
                group => group.transaction!.Id,
                adjust => adjust.TransactionId,
                (group, adjust) => new
                {
                    group.p,
                    group.transaction,
                    group.transaction!.Wallet,
                    group.transaction.Amount,
                    adjust
                })
            .SelectMany(
                x => x.adjust.DefaultIfEmpty(),
                (x, adjust) => new
                {
                    x.p,
                    x.transaction,
                    x.Wallet,
                    x.Amount,
                    AdjustType = adjust == null ? 0 : adjust.Type,
                    IncludeInNetProfit = adjust == null || adjust.IncludeInNetProfit
                })
            .GroupBy(
                x => new
                {
                    x.p.BrandId,
                    x.p.AgentId,
                    DividendPercentage = x.p.Agent == null ? (decimal?)null : x.p.Agent.DividendPercentage,
                    Path = x.p.Agent == null ? (LTree?)null : x.p.Agent.Path,
                    x.p.PlayerCode
                },
                (x, players) => new
                {
                    BrandId = x.BrandId,
                    Hour = start.Hour,
                    ReportDate = reportDate.ToUniversalTime(),
                    AgentId = x.AgentId,
                    PlayerCode = x.PlayerCode,
                    DividendPercentage = x.DividendPercentage,
                    x.Path,
                    ChiefAgentId = x.Path == null ? (LTree?)null : x.Path.Value.Subpath(0, 1),
                    RegisteredCount = players.Count(p => p.p.RegisteredOn >= utcStart
                        && p.p.RegisteredOn < utcEnd) > 0
                        ? 1
                        : 0,
                    FirstFundInCount = players.Count(p => p.p.FirstDepositOn >= utcStart
                        && p.p.FirstDepositOn < utcEnd) > 0
                        ? 1
                        : 0,
                    DepositAmount = players
                        .Where(p => p.transaction != null
                            && new[] { (short)TransactionType.Deposit, (short)TransactionType.Deposited }.Contains(
                                p.transaction.Type))
                        .Sum(t => t.transaction!.Amount + t.transaction!.LockAmount),
                    WithdrawalAmount = players
                        .Where(p => p.transaction != null
                            && new[] { (short)TransactionType.Withdrawal, (short)TransactionType.WithdrawCommit }
                                .Contains(p.transaction.Type))
                        .Sum(t => t.transaction!.Amount + t.transaction!.LockAmount),
                    CompanyAdjustAmount = players
                        .Where(p => p.transaction != null
                            && p.transaction.Type == (short)TransactionType.CompanyAdjust)
                        .Sum(t => t.transaction!.Amount + t.transaction!.LockAmount),
                    AgentAdjustAmount = players
                        .Where(p => p.transaction != null
                            && p.transaction.Type == (short)TransactionType.AgentAdjust)
                        .Sum(t => t.transaction!.Amount + t.transaction!.LockAmount),
                    SystemAdjustAmount = players
                        .Where(p => p.transaction != null
                            && p.transaction.Type == (short)TransactionType.SystemAdjust)
                        .Sum(t => t.transaction!.Amount + t.transaction!.LockAmount),
                    BonusAdjustAmount = players
                        .Where(p => p.transaction != null
                            && p.transaction.Type == (short)TransactionType.BonusAdjust)
                        .Sum(t => t.transaction!.Amount + t.transaction!.LockAmount),
                    BonusAmount = players
                        .Where(p => p.transaction != null
                            && p.transaction.Type == (short)TransactionType.Bonus)
                        .Sum(t => t.transaction!.Amount + t.transaction!.LockAmount),
                    RebateAmount = players
                        .Where(p => p.transaction != null
                            && p.transaction.Type == (short)TransactionType.Rebate)
                        .Sum(t => t.transaction!.Amount + t.transaction!.LockAmount),
                    DividendAmount = players
                        .Where(p => p.transaction != null
                            && p.transaction.Type == (short)TransactionType.Dividend)
                        .Sum(t => t.transaction!.Amount + t.transaction!.LockAmount),
                    PromotionAmount = players
                        .Where(p => p.transaction != null
                            && p.transaction.Type == (short)TransactionType.Promotion)
                        .Sum(t => t.transaction!.Amount + t.transaction!.LockAmount),
                    AllAgentRebateAmount = players
                        .Where(p => p.transaction != null
                            && p.transaction.Type == (short)TransactionType.DividendAllAgentRebate)
                        .Sum(t => t.transaction!.Amount + t.transaction!.LockAmount),
                    NoIncludeNetProfitCompanyAdjustAmount = players
                        .Where(p => p.transaction != null
                            && p.transaction.Type == (short)TransactionType.CompanyAdjust
                            && p.IncludeInNetProfit == false
                            && p.AdjustType == (short)PlayerAdjustmentTransactionType.CompanyAdjust)
                        .Sum(t => t.transaction!.Amount + t.transaction!.LockAmount),
                    NoIncludeNetProfitAgentAdjustAmount = players
                        .Where(p => p.transaction != null
                            && p.transaction.Type == (short)TransactionType.AgentAdjust
                            && p.IncludeInNetProfit == false
                            && p.AdjustType == (short)PlayerAdjustmentTransactionType.AgentAdjust)
                        .Sum(t => t.transaction!.Amount + t.transaction!.LockAmount),
                    NoIncludeNetProfitBonusAdjustAmount = players
                        .Where(p => p.transaction != null
                            && p.transaction.Type == (short)TransactionType.BonusAdjust
                            && p.IncludeInNetProfit == false
                            && p.AdjustType == (short)PlayerAdjustmentTransactionType.BonusAdjust)
                        .Sum(t => t.transaction!.Amount + t.transaction!.LockAmount),
                    UpgradeAmount = players
                        .Where(p => p.transaction != null
                            && p.transaction.Type == (short)TransactionType.PromotionVipLevelUpgrade)
                        .Sum(t => t.transaction!.Amount + t.transaction!.LockAmount),
                    SeasonAmount = players
                        .Where(p => p.transaction != null
                            && p.transaction.Type == (short)TransactionType.PromotionVipLevelSeason)
                        .Sum(t => t.transaction!.Amount + t.transaction!.LockAmount),
                    BirthdayAmount = players
                        .Where(p => p.transaction != null
                            && p.transaction.Type == (short)TransactionType.PromotionVipLevelBirthday)
                        .Sum(t => t.transaction!.Amount + t.transaction!.LockAmount)
                }).ToArrayAsync();

        var brands = playerTotals.Select(x => x.BrandId).Distinct();

        var agents = await _repository.Agents
            .Where(x => brands.Contains(x.BrandId))
            .ToArrayAsync().ConfigureAwait(false);

        var playerAggregate = playerTotals.Select(x => new PlayerAggregateModel
        {
            BrandId = x.BrandId,
            Hour = x.Hour,
            ReportDate = x.ReportDate,
            AgentId = x.AgentId,
            PlayerCode = x.PlayerCode,
            RegisteredCount = x.RegisteredCount,
            FirstFundInCount = x.FirstFundInCount,
            DepositAmount = x.DepositAmount,
            WithdrawalAmount = x.WithdrawalAmount,
            CompanyAdjustAmount = x.CompanyAdjustAmount,
            AgentAdjustAmount = x.AgentAdjustAmount,
            SystemAdjustAmount = x.SystemAdjustAmount,
            BonusAdjustAmount = x.BonusAdjustAmount,
            BonusAmount = x.BonusAmount,
            RebateAmount = x.RebateAmount,
            DividendAmount = x.DividendAmount,
            PromotionAmount = x.PromotionAmount,
            AllAgentRebateAmount = x.AllAgentRebateAmount,
            NoIncludeNetProfitCompanyAdjustAmount = x.NoIncludeNetProfitCompanyAdjustAmount,
            NoIncludeNetProfitAgentAdjustAmount = x.NoIncludeNetProfitAgentAdjustAmount,
            NoIncludeNetProfitBonusAdjustAmount = x.NoIncludeNetProfitBonusAdjustAmount,
            AgentDividendPercentage = x.DividendPercentage,
            AgentDividendPercentageTreeData = GenerateAgentDividendPercentages(x.Path,agents),
            ChiefAgentDividendPercentage = x.ChiefAgentId == null ? null :
                agents.FirstOrDefault(c => c.Id == Guid.Parse(x.ChiefAgentId))?.DividendPercentage
        }).ToArray();

        foreach (var player in playerAggregate)
        {
            var playerRoundAggregates = roundAggregates
                .Where(r => r.PlayerCode == player.PlayerCode)
                .ToArray();

            var playerRoundAggregate = playerRoundAggregates.FirstOrDefault();

            if (playerRoundAggregate != null)
            {
                player.AgentId = playerRoundAggregate.AgentId;
            }

            player.BetAmount = playerRoundAggregates.Sum(r => r.BetAmount);
            player.OriginalTurnover = playerRoundAggregates.Sum(r => r.OriginalTurnover);
            player.ValidBetAmount = playerRoundAggregates.Sum(r => r.ValidBetAmount);
            player.Turnover = playerRoundAggregates.Sum(r => r.Turnover);
            player.WinLossAmount = playerRoundAggregates.Sum(r => r.WinLoss);
        }

        var playerBonuses = playerTotals
            .Where(x =>
                x.RebateAmount != 0
                || x.PromotionAmount != 0
                || x.UpgradeAmount != 0
                || x.SeasonAmount != 0
                || x.BirthdayAmount != 0
            )
            .Select(x => new PlayerBonusAggregateModel
            {
                BrandId = x.BrandId,
                AgentId = x.AgentId,
                PlayerCode = x.PlayerCode,
                Hour = start.Hour,
                ReportDate = x.ReportDate,
                RebateAmount = x.RebateAmount,
                PromotionAmount = x.PromotionAmount,
                UpgradeAmount = x.UpgradeAmount,
                SeasonAmount = x.SeasonAmount,
                BirthdayAmount = x.BirthdayAmount
            }).ToArray();

        var playerAggregateByGameType = roundAggregates
            .GroupBy(
                x => new
                {
                    x.BrandId,
                    x.AgentId,
                    x.PlayerCode,
                    x.GameProviderId,
                    x.GameId,
                    x.GameType,
                    x.VipLevel,
                    x.VipSportsRebatePercentage,
                    x.VipVirtualSportsRebatePercentage,
                    x.VipLiveDealerRebatePercentage,
                    x.VipCardGameRebatePercentage,
                    x.VipLotteryRebatePercentage,
                    x.VipSlotRebatePercentage
                },
                (x, aggregate) => new PlayerGameTypeAggregateModel
                {
                    BrandId = x.BrandId,
                    AgentId = x.AgentId,
                    PlayerCode = x.PlayerCode,
                    VipLevel = x.VipLevel,
                    GameProviderId = x.GameProviderId,
                    GameId = x.GameId,
                    GameType = (GameType)x.GameType,
                    Hour = start.Hour,
                    VipSportsRebatePercentage = x.VipSportsRebatePercentage,
                    VipVirtualSportsRebatePercentage = x.VipVirtualSportsRebatePercentage,
                    VipLiveDealerRebatePercentage = x.VipLiveDealerRebatePercentage,
                    VipCardGameRebatePercentage = x.VipCardGameRebatePercentage,
                    VipLotteryRebatePercentage = x.VipLotteryRebatePercentage,
                    VipSlotRebatePercentage = x.VipSlotRebatePercentage,
                    BetAmount = aggregate.Sum(a => a.BetAmount),
                    ValidBetAmount = aggregate.Sum(a => a.ValidBetAmount),
                    OriginalTurnover = aggregate.Sum(a => a.OriginalTurnover),
                    Turnover = aggregate.Sum(a => a.Turnover),
                    WinLoss = aggregate.Sum(a => a.WinLoss),
                    VipRebateAmount = GetPlayerRebateAmountByGameType(new VipLevel
                        {
                            SportsRebatePercentage = x.VipSportsRebatePercentage,
                            VirtualSportsRebatePercentage = x.VipVirtualSportsRebatePercentage,
                            LiveDealerRebatePercentage = x.VipLiveDealerRebatePercentage,
                            CardGameRebatePercentage = x.VipCardGameRebatePercentage,
                            LotteryRebatePercentage = x.VipLotteryRebatePercentage,
                            SlotRebatePercentage = x.VipSlotRebatePercentage
                        },
                            (short)x.GameType,
                            aggregate!.Select(a => ((short)a.GameType,a.Turnover))),
                    CreatedOn = DateTimeOffset.UtcNow,
                    ReportDate = reportDate.ToUniversalTime()
                }).ToArray();

        var players = playerAggregateByGameType.Select(x => x.PlayerCode);

        playerAggregate = playerAggregate.Where(x => x.RegisteredCount != 0
            || x.FirstFundInCount != 0
            || x.DepositAmount != 0
            || x.WithdrawalAmount != 0
            || x.CompanyAdjustAmount != 0
            || x.BonusAdjustAmount != 0
            || x.BonusAmount != 0
            || x.RebateAmount != 0
            || x.DividendAmount != 0
            || x.PromotionAmount != 0
            || x.BetAmount != 0
            || x.OriginalTurnover != 0
            || x.ValidBetAmount != 0
            || x.Turnover != 0
            || x.AgentAdjustAmount != 0
            || x.NoIncludeNetProfitAgentAdjustAmount != 0
            || x.NoIncludeNetProfitCompanyAdjustAmount != 0
            || x.NoIncludeNetProfitBonusAdjustAmount != 0
            || x.WinLossAmount != 0
            || players.Contains(x.PlayerCode)).ToArray();

        return (playerAggregate, playerAggregateByGameType, playerBonuses);
    }


    private static decimal GetPlayerRebateAmountByGameType(
        VipLevel vip, short type,
        IEnumerable<(short GameType,decimal Turnover)> roundAggregates)
    {
        return (GameType)type switch
        {
            GameType.Sportsbook or GameType.ESports => vip.SportsRebatePercentage * roundAggregates
                .Where(a => a.GameType is (short)GameType.Sportsbook or (short)GameType.ESports)
                .Sum(a => a.Turnover) / 100M,
            GameType.VirtualSports => vip.VirtualSportsRebatePercentage * roundAggregates
                .Where(a => a.GameType == (short)GameType.VirtualSports)
                .Sum(a => a.Turnover) / 100M,
            GameType.LiveDealer => vip.LiveDealerRebatePercentage * roundAggregates
                .Where(a => a.GameType == (short)GameType.LiveDealer)
                .Sum(a => a.Turnover) / 100M,
            GameType.CardGame => vip.CardGameRebatePercentage * roundAggregates
                .Where(a => a.GameType == (short)GameType.CardGame)
                .Sum(a => a.Turnover) / 100M,
            GameType.Fishing or GameType.Arcade or GameType.Slot or GameType.TableGame => vip.SlotRebatePercentage *
                roundAggregates
                    .Where(a => a.GameType is (short)GameType.Fishing
                        or (short)GameType.Arcade
                        or (short)GameType.Slot
                        or (short)GameType.TableGame)
                    .Sum(a => a.Turnover) / 100M,
            GameType.Lottery => vip.LotteryRebatePercentage * roundAggregates
                .Where(a => a.GameType == (short)GameType.Lottery)
                .Sum(a => a.Turnover) / 100M,
            _ => 0M
        };
    }

    private static string? GenerateAgentDividendPercentages(LTree? agentPath, Agent[] agents)
    {
        if (agentPath is null)
        {
            return null;
        }

        var agentIds = agentPath.ToString()!.Split(".").Select(Guid.Parse);
        var agentTree = agents.Where(x => agentIds.Contains(x.Id)).Select(x => new AgentDividendPercentageData
        {
            AgentId = x.Id,
            ParentAgentId = x.ParentAgentId,
            DividendPercentage = x.DividendPercentage,
        });
        return JsonSerializer.Serialize(agentTree);
    }
}