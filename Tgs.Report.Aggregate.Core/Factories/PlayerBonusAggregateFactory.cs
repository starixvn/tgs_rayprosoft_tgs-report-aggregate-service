using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

namespace Tgs.Report.Aggregate.Core.Factories;

public class PlayerBonusAggregateFactory(ITgsRepository repository) : IPlayerBonusAggregateFactory
{
    public async ValueTask<int> CreatePlayerBonusAggregatesAsync(
        PlayerBonusAggregateModel[] aggregate,
        CancellationToken cancellationToken)
    {
        _ = repository.PlayerBonusAggregates.AddRangeAsync(aggregate.Select(model => new PlayerBonusAggregate
                {
                    BrandId = model.BrandId,
                    AgentId = model.AgentId,
                    PlayerCode = model.PlayerCode,
                    Hour = model.Hour,
                    ReportDate = model.ReportDate.UtcDateTime,
                    ReportDateTime = model.ReportDateTime.UtcDateTime,
                    CreatedOn = DateTime.UtcNow,
                    RebateAmount = model.RebateAmount,
                    PromotionAmount = model.PromotionAmount,
                    UpgradeAmount = model.UpgradeAmount,
                    SeasonAmount = model.SeasonAmount,
                    BirthdayAmount = model.BirthdayAmount
                }),
                cancellationToken)
            .ConfigureAwait(false);

        return await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}