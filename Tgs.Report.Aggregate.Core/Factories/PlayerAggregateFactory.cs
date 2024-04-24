using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Tgs.BrandCertificate.Clients;
using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;
using Tgs.Report.Aggregate.Domain.Models.PlayerGameAggregate;
using Tgs.Report.Aggregate.Domain.Shared;

namespace Tgs.Report.Aggregate.Core.Factories;

public class PlayerAggregateFactory(
    ITgsRepository repository,
    ITgsService<RoundAggregate> roundAggregateRepository,
    ITgsService<Player> playerRepository,
    ITgsService<Transaction> transactionRepository,
    ITgsService<AdjustPlayerRecord> adjustPlayerRecordRepository,
    IBrandServiceClient brandServiceClient)
    : IPlayerAggregateFactory
{
    private static readonly short[] _TransactionTypes =
    {
        (short)TransactionType.Deposit,
        (short)TransactionType.Deposited,
        (short)TransactionType.Withdrawal,
        (short)TransactionType.WithdrawCommit,
        (short)TransactionType.CompanyAdjust,
        (short)TransactionType.AgentAdjust,
        (short)TransactionType.SystemAdjust,
        (short)TransactionType.BonusAdjust,
        (short)TransactionType.Bonus,
        (short)TransactionType.Rebate,
        (short)TransactionType.Dividend,
        (short)TransactionType.Promotion,
        (short)TransactionType.DividendAllAgentRebate,
        (short)TransactionType.CompanyAdjust,
        (short)TransactionType.AgentAdjust,
        (short)TransactionType.BonusAdjust,
        (short)TransactionType.PromotionVipLevelUpgrade,
        (short)TransactionType.PromotionVipLevelSeason,
        (short)TransactionType.PromotionVipLevelBirthday
    };

    public Task CreatePlayerAggregatesAsync(PlayerAggregateModel[] models)
    {
        _ = repository.PlayerAggregates.AddRangeAsync(models.Select(model => new PlayerAggregate
        {
            BrandId = model.BrandId,
            AgentId = model.AgentId,
            PlayerCode = model.PlayerCode,
            Hour = model.Hour,
            ReportDate = model.ReportDate.UtcDateTime,
            ReportDateTime = model.ReportDateTime.UtcDateTime,
            CreatedOn = DateTime.UtcNow,
            RegisteredCount = model.RegisteredCount,
            FirstFundInCount = model.FirstFundInCount,
            DepositAmount = model.DepositAmount,
            WithdrawalAmount = model.WithdrawalAmount,
            CompanyAdjustAmount = model.CompanyAdjustAmount,
            AgentAdjustAmount = model.AgentAdjustAmount,
            SystemAdjustAmount = model.SystemAdjustAmount,
            BonusAdjustAmount = model.BonusAdjustAmount,
            BonusAmount = model.BonusAmount,
            RebateAmount = model.RebateAmount,
            PromotionAmount = model.PromotionAmount,
            DividendAmount = model.DividendAmount,
            BetAmount = model.BetAmount,
            ValidBetAmount = model.ValidBetAmount,
            OriginalTurnover = model.OriginalTurnover,
            Turnover = model.Turnover,
            WinLossAmount = model.WinLossAmount,
            AllAgentRebateAmount = model.AllAgentRebateAmount,
            NoIncludeNetProfitCompanyAdjustAmount = model.NoIncludeNetProfitCompanyAdjustAmount,
            NoIncludeNetProfitAgentAdjustAmount = model.NoIncludeNetProfitAgentAdjustAmount,
            NoIncludeNetProfitBonusAdjustAmount = model.NoIncludeNetProfitBonusAdjustAmount,
            AgentDividendPercentage = model.AgentDividendPercentage,
            ChiefAgentDividendPercentage = model.ChiefAgentDividendPercentage,
            AgentDividendPercentageTreeData = model.AgentDividendPercentageTreeData,
            RoundCount = model.RoundCount,
            SeasonAmount = model.SeasonAmount,
            UpgradeAmount = model.UpgradeAmount,
            BirthdayAmount = model.BirthdayAmount,
            PlatformChargePercentage = model.PlatformChargePercentage
        }));

        return repository.SaveChangesAsync();
    }

    public async
        ValueTask<(PlayerAggregateModel[] playerAggregate, PlayerGameTypeAggregateModel[] playerAggregateByGameType,
            PlayerBonusAggregateModel[] playerBonusAggregate)>
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

        var roundAggregates = await roundAggregateRepository.GetQueries()
            .Where(x => x.ReportDate == reportDate.ToUniversalTime()
                && x.Hour == start.Hour
                && x.Minute == start.Minute)
            .Join(
                repository.Players.Include(x => x.Agent)
                    .Include(x => x.Vip),
                r => r.PlayerCode,
                p => p.PlayerCode,
                (r, p) => new
                {
                    r,
                    p.Vip,
                    p.Agent
                })
            .Select(x => new AggregateModel
            {
                BrandId = x.r.BrandId,
                AgentId = x.r.AgentId,
                PlayerCode = x.r.PlayerCode,
                VipLevel = x.Vip == null ? null : x.Vip.Level,
                GameProviderId = x.r.GameProviderId,
                GameId = x.r.GameId,
                GameType = (GameType)x.r.GameType,
                AgentDividendPercentage = x.Agent == null ? null : x.Agent.DividendPercentage,
                Path = x.Agent == null ? (LTree?)null : x.Agent.Path,
                ChiefAgentId = x.Agent == null ? (LTree?)null : x.Agent.Path.Subpath(0, 1),
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
                WinLossAmount = x.r.WinLoss,
                RoundCount = x.r.RoundCount,
                ReportDate = reportDate.ToUniversalTime()
            })
            .ToArrayAsync();

        var playerTotals = await transactionRepository.GetQueries()
            .Where(x => x.RecordedOn >= utcStart && x.RecordedOn < utcEnd)
            .Where(x => _TransactionTypes.Contains(x.Type))
            .Join(playerRepository.GetQueries().Include(x => x.Agent),
                t => t.PlayerCode,
                p => p.PlayerCode,
                (t, p) => new
                {
                    Transaction = t,
                    Player = p
                })
            .GroupJoin(
                adjustPlayerRecordRepository.GetQueries()
                    .Where(x => x.CreatedOn >= utcStart),
                t => t.Transaction.Id,
                adjust => adjust.TransactionId,
                (t, adjust) => new
                {
                    t.Transaction,
                    t.Player,
                    adjust
                })
            .SelectMany(
                x => x.adjust.DefaultIfEmpty(),
                (x, adjust) => new
                {
                    x.Transaction,
                    x.Player,
                    AdjustType = adjust == null ? 0 : adjust.Type,
                    IncludeInNetProfit = adjust == null || adjust.IncludeInNetProfit
                })
            .GroupBy(
                x => new
                {
                    x.Transaction.BrandId,
                    x.Player.AgentId,
                    DividendPercentage = x.Player.Agent == null ? (decimal?)null : x.Player.Agent.DividendPercentage,
                    Path = x.Player.Agent == null ? (LTree?)null : x.Player.Agent.Path,
                    x.Player.PlayerCode
                },
                (x, players) => new AggregateModel
                {
                    BrandId = x.BrandId,
                    Hour = start.Hour,
                    ReportDate = reportDate.ToUniversalTime(),
                    AgentId = x.AgentId,
                    PlayerCode = x.PlayerCode,
                    AgentDividendPercentage = x.DividendPercentage,
                    Path = x.Path,
                    ChiefAgentId = x.Path == null ? (LTree?)null : x.Path.Value.Subpath(0, 1),
                    DepositAmount = players
                        .Where(p => new[] { (short)TransactionType.Deposit, (short)TransactionType.Deposited }.Contains(
                            p.Transaction.Type))
                        .Sum(t => t.Transaction!.Amount + t.Transaction!.LockAmount),
                    WithdrawalAmount = players
                        .Where(p => new[] { (short)TransactionType.Withdrawal, (short)TransactionType.WithdrawCommit }
                            .Contains(p.Transaction.Type))
                        .Sum(t => t.Transaction!.Amount + t.Transaction!.LockAmount),
                    CompanyAdjustAmount = players
                        .Where(p => p.Transaction.Type == (short)TransactionType.CompanyAdjust)
                        .Sum(t => t.Transaction!.Amount + t.Transaction!.LockAmount),
                    AgentAdjustAmount = players
                        .Where(p => p.Transaction.Type == (short)TransactionType.AgentAdjust)
                        .Sum(t => t.Transaction!.Amount + t.Transaction!.LockAmount),
                    SystemAdjustAmount = players
                        .Where(p => p.Transaction.Type == (short)TransactionType.SystemAdjust)
                        .Sum(t => t.Transaction!.Amount + t.Transaction!.LockAmount),
                    BonusAdjustAmount = players
                        .Where(p => p.Transaction.Type == (short)TransactionType.BonusAdjust)
                        .Sum(t => t.Transaction!.Amount + t.Transaction!.LockAmount),
                    BonusAmount = players
                        .Where(p => p.Transaction.Type == (short)TransactionType.Bonus)
                        .Sum(t => t.Transaction!.Amount + t.Transaction!.LockAmount),
                    RebateAmount = players
                        .Where(p => p.Transaction.Type == (short)TransactionType.Rebate)
                        .Sum(t => t.Transaction!.Amount + t.Transaction!.LockAmount),
                    DividendAmount = players
                        .Where(p => p.Transaction.Type == (short)TransactionType.Dividend)
                        .Sum(t => t.Transaction!.Amount + t.Transaction!.LockAmount),
                    PromotionAmount = players
                        .Where(p => p.Transaction.Type == (short)TransactionType.Promotion)
                        .Sum(t => t.Transaction!.Amount + t.Transaction!.LockAmount),
                    AllAgentRebateAmount = players
                        .Where(p => p.Transaction.Type == (short)TransactionType.DividendAllAgentRebate)
                        .Sum(t => t.Transaction!.Amount + t.Transaction!.LockAmount),
                    NoIncludeNetProfitCompanyAdjustAmount = players
                        .Where(p => p.Transaction.Type == (short)TransactionType.CompanyAdjust
                            && p.IncludeInNetProfit == false
                            && p.AdjustType == (short)PlayerAdjustmentTransactionType.CompanyAdjust)
                        .Sum(t => t.Transaction!.Amount + t.Transaction!.LockAmount),
                    NoIncludeNetProfitAgentAdjustAmount = players
                        .Where(p => p.Transaction.Type == (short)TransactionType.AgentAdjust
                            && p.IncludeInNetProfit == false
                            && p.AdjustType == (short)PlayerAdjustmentTransactionType.AgentAdjust)
                        .Sum(t => t.Transaction!.Amount + t.Transaction!.LockAmount),
                    NoIncludeNetProfitBonusAdjustAmount = players
                        .Where(p => p.Transaction.Type == (short)TransactionType.BonusAdjust
                            && p.IncludeInNetProfit == false
                            && p.AdjustType == (short)PlayerAdjustmentTransactionType.BonusAdjust)
                        .Sum(t => t.Transaction!.Amount + t.Transaction!.LockAmount),
                    UpgradeAmount = players
                        .Where(p => p.Transaction.Type == (short)TransactionType.PromotionVipLevelUpgrade)
                        .Sum(t => t.Transaction!.Amount + t.Transaction!.LockAmount),
                    SeasonAmount = players
                        .Where(p => p.Transaction.Type == (short)TransactionType.PromotionVipLevelSeason)
                        .Sum(t => t.Transaction!.Amount + t.Transaction!.LockAmount),
                    BirthdayAmount = players
                        .Where(p => p.Transaction.Type == (short)TransactionType.PromotionVipLevelBirthday)
                        .Sum(t => t.Transaction!.Amount + t.Transaction!.LockAmount)
                }).ToArrayAsync();

        var playerRegisters = await playerRepository.GetQueries()
            .Include(x => x.Agent)
            .Include(x => x.Vip)
            .Where(x => x.RegisteredOn >= utcStart
                && x.RegisteredOn < utcEnd
                || x.FirstDepositOn >= utcStart
                && x.FirstDepositOn < utcEnd)
            .Select(x => new AggregateModel
            {
                BrandId = x.BrandId,
                Hour = start.Hour,
                ReportDate = reportDate.ToUniversalTime(),
                AgentId = x.AgentId,
                PlayerCode = x.PlayerCode,
                AgentDividendPercentage = x.Agent == null ? null : x.Agent.DividendPercentage,
                Path = x.Agent == null ? (LTree?)null : x.Agent.Path,
                ChiefAgentId = x.Agent == null ? (LTree?)null : x.Agent.Path.Subpath(0, 1),
                RegisteredCount = x.RegisteredOn != null && x.RegisteredOn >= utcStart && x.RegisteredOn < utcEnd
                    ? 1
                    : 0,
                FirstFundInCount = x.FirstDepositOn != null && x.FirstDepositOn >= utcStart && x.FirstDepositOn < utcEnd
                    ? 1
                    : 0
            })
            .ToArrayAsync()
            .ConfigureAwait(false);

        var playerData = roundAggregates.Concat(playerTotals).Concat(playerRegisters).ToArray();

        var brandIds = playerData.Select(x => x.BrandId).Distinct();

        var brandInfos = brandServiceClient.GetBrandAgentDividendInfosAsync();

        var platformCharges = await brandInfos.Where(x => brandIds.Contains(x.Id)).ToArrayAsync();

        var agents = await repository.Agents
            .Where(x => brandIds.Contains(x.BrandId))
            .Select(x => new AgentDividendPercentageModel
            {
                AgentId = x.Id,
                ParentAgentId = x.ParentAgentId,
                DividendPercentage = x.DividendPercentage
            })
            .ToArrayAsync().ConfigureAwait(false);

        var playerAggregate = playerData
            .GroupBy(x => new
            {
                x.BrandId,
                x.AgentId,
                x.AgentDividendPercentage,
                x.ChiefAgentId,
                x.Path,
                x.PlayerCode
            }, (x, list) => new PlayerAggregateModel
            {
                BrandId = x.BrandId,
                Hour = start.Hour,
                ReportDate = reportDate.ToUniversalTime(),
                ReportDateTime = utcStart,
                AgentId = x.AgentId,
                PlayerCode = x.PlayerCode,
                RegisteredCount = list.Sum(l => l.RegisteredCount),
                FirstFundInCount = list.Sum(l => l.FirstFundInCount),
                DepositAmount = list.Sum(l => l.DepositAmount),
                WithdrawalAmount = list.Sum(l => l.WithdrawalAmount),
                CompanyAdjustAmount = list.Sum(l => l.CompanyAdjustAmount),
                AgentAdjustAmount = list.Sum(l => l.AgentAdjustAmount),
                SystemAdjustAmount = list.Sum(l => l.SystemAdjustAmount),
                BonusAdjustAmount = list.Sum(l => l.BonusAdjustAmount),
                BonusAmount = list.Sum(l => l.BonusAmount),
                RebateAmount = list.Sum(l => l.RebateAmount),
                DividendAmount = list.Sum(l => l.DividendAmount),
                PromotionAmount = list.Sum(l => l.PromotionAmount),
                AllAgentRebateAmount = list.Sum(l => l.AllAgentRebateAmount),
                NoIncludeNetProfitCompanyAdjustAmount = list.Sum(l => l.NoIncludeNetProfitCompanyAdjustAmount),
                NoIncludeNetProfitAgentAdjustAmount = list.Sum(l => l.NoIncludeNetProfitAgentAdjustAmount),
                NoIncludeNetProfitBonusAdjustAmount = list.Sum(l => l.NoIncludeNetProfitBonusAdjustAmount),
                BetAmount = list.Sum(l => l.BetAmount),
                OriginalTurnover = list.Sum(l => l.OriginalTurnover),
                Turnover = list.Sum(l => l.Turnover),
                WinLossAmount = list.Sum(l => l.WinLossAmount),
                AgentDividendPercentage = x.AgentDividendPercentage ?? 0M,
                AgentDividendPercentageTreeData = GenerateAgentDividendPercentages(x.Path, agents),
                ChiefAgentDividendPercentage = x.ChiefAgentId == null
                    ? 0M
                    : agents.FirstOrDefault(c => c.AgentId == Guid.Parse(x.ChiefAgentId))?.DividendPercentage,
                RoundCount = list.Sum(l => l.RoundCount),
                SeasonAmount = list.Sum(l => l.SeasonAmount),
                UpgradeAmount = list.Sum(l => l.UpgradeAmount),
                BirthdayAmount = list.Sum(l => l.BirthdayAmount)
            }).ToArray();

        foreach (var playerAggregateModel in playerAggregate)
        {
            playerAggregateModel.PlatformChargePercentage = platformCharges?
                .FirstOrDefault(x => x.Id == playerAggregateModel.BrandId)?
                .PlatformCharge ?? 0M;
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
                ReportDateTime = utcStart,
                RebateAmount = x.RebateAmount,
                PromotionAmount = x.PromotionAmount,
                UpgradeAmount = x.UpgradeAmount,
                SeasonAmount = x.SeasonAmount,
                BirthdayAmount = x.BirthdayAmount
            }).ToArray();

        var playerAggregateByGameType = roundAggregates
            .GroupJoin(
                repository.AgentProductFees,
                a => a.AgentId,
                f => f.AgentId,
                (agg, fees) => new
                {
                    agg,
                    fees
                })
            .SelectMany(
                x => x.fees.DefaultIfEmpty(),
                (x, fee) => new
                {
                    x.agg,
                    fee
                })
            .GroupBy(
                x => new
                {
                    x.agg.BrandId,
                    x.agg.AgentId,
                    x.agg.PlayerCode,
                    x.agg.GameProviderId,
                    x.agg.GameId,
                    x.agg.GameType,
                    x.agg.VipLevel,
                    x.agg.VipSportsRebatePercentage,
                    x.agg.VipVirtualSportsRebatePercentage,
                    x.agg.VipLiveDealerRebatePercentage,
                    x.agg.VipCardGameRebatePercentage,
                    x.agg.VipLotteryRebatePercentage,
                    x.agg.VipSlotRebatePercentage,
                    x.fee
                },
                (x, aggregate) => new PlayerGameTypeAggregateModel
                {
                    BrandId = x.BrandId,
                    AgentId = x.AgentId,
                    PlayerCode = x.PlayerCode,
                    VipLevel = x.VipLevel,
                    GameProviderId = x.GameProviderId,
                    GameId = x.GameId,
                    GameType = x.GameType,
                    Hour = start.Hour,
                    VipSportsRebatePercentage = x.VipSportsRebatePercentage,
                    VipVirtualSportsRebatePercentage = x.VipVirtualSportsRebatePercentage,
                    VipLiveDealerRebatePercentage = x.VipLiveDealerRebatePercentage,
                    VipCardGameRebatePercentage = x.VipCardGameRebatePercentage,
                    VipLotteryRebatePercentage = x.VipLotteryRebatePercentage,
                    VipSlotRebatePercentage = x.VipSlotRebatePercentage,
                    BetAmount = aggregate.Sum(a => a.agg.BetAmount),
                    ValidBetAmount = aggregate.Sum(a => a.agg.ValidBetAmount),
                    OriginalTurnover = aggregate.Sum(a => a.agg.OriginalTurnover),
                    Turnover = aggregate.Sum(a => a.agg.Turnover),
                    WinLoss = aggregate.Sum(a => a.agg.WinLossAmount),
                    VipRebateAmount = GetPlayerRebateAmountByGameType(new()
                        {
                            SportsRebatePercentage = x.VipSportsRebatePercentage,
                            VirtualSportsRebatePercentage = x.VipVirtualSportsRebatePercentage,
                            LiveDealerRebatePercentage = x.VipLiveDealerRebatePercentage,
                            CardGameRebatePercentage = x.VipCardGameRebatePercentage,
                            LotteryRebatePercentage = x.VipLotteryRebatePercentage,
                            SlotRebatePercentage = x.VipSlotRebatePercentage
                        },
                        (short)x.GameType,
                        aggregate.Select(a => ((short)a.agg.GameType, a.agg.Turnover))),
                    CreatedOn = DateTimeOffset.UtcNow,
                    ReportDate = reportDate.UtcDateTime,
                    ReportDateTime = utcStart,
                    RoundCount = aggregate.Sum(a => a.agg.RoundCount),
                    ProductFeePercentage = x.GameType switch
                    {
                        GameType.CardGame => x.fee?.CardGamePercentage ?? 0M,
                        GameType.LiveDealer => x.fee?.LiveDealerPercentage ?? 0M,
                        GameType.Lottery => x.fee?.LotteryPercentage ?? 0M,
                        GameType.Arcade => x.fee?.SlotPercentage ?? 0M,
                        GameType.Fishing => x.fee?.SlotPercentage ?? 0M,
                        GameType.Slot => x.fee?.SlotPercentage ?? 0M,
                        GameType.TableGame => x.fee?.SlotPercentage ?? 0M,
                        GameType.Sportsbook => x.fee?.SportsPercentage ?? 0M,
                        GameType.ESports => x.fee?.SportsPercentage ?? 0M,
                        GameType.VirtualSports => x.fee?.VirtualSportsPercentage ?? 0M,
                        _ => 0M
                    }
                }).ToArray();

        foreach (var playerGameTypeAggregateModel in playerAggregateByGameType)
        {
            playerGameTypeAggregateModel.PlatformChargePercentage = platformCharges?
                .FirstOrDefault(x => x.Id == playerGameTypeAggregateModel.BrandId)?
                .PlatformCharge ?? 0M;
        }

        return (playerAggregate, playerAggregateByGameType, playerBonuses);
    }

    private static decimal GetPlayerRebateAmountByGameType(
        VipLevel vip, short type,
        IEnumerable<(short GameType, decimal Turnover)> roundAggregates)
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

    private static string? GenerateAgentDividendPercentages(LTree? agentPath, AgentDividendPercentageModel[] agents)
    {
        if (agentPath is null)
        {
            return null;
        }

        var agentIds = agentPath.ToString()!.Split(".").Select(Guid.Parse);
        var agentTree = agents.Where(x => agentIds.Contains(x.AgentId)).Select(x => new AgentDividendPercentageData
        {
            AgentId = x.AgentId,
            ParentAgentId = x.ParentAgentId,
            DividendPercentage = x.DividendPercentage
        });
        return JsonSerializer.Serialize(agentTree);
    }
}