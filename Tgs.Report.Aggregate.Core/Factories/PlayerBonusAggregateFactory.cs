using Tgs.Report.Aggregate.Core.Extensions;
using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

namespace Tgs.Report.Aggregate.Core.Factories;

public class PlayerBonusAggregateFactory : IPlayerBonusAggregateFactory
{
    private readonly ITgsRepository _repository;

    public PlayerBonusAggregateFactory(ITgsRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<int> CreatePlayerBonusAggregatesAsync(
        PlayerBonusAggregateModel[] aggregate, CancellationToken cancellationToken)
    {
        _ = _repository.PlayerBonusAggregates.AddRangeAsync(aggregate.Select(model => new PlayerBonusAggregate
        {
            BrandId = model.BrandId,
            AgentId = model.AgentId,
            PlayerCode = model.PlayerCode,
            Hour = model.Hour,
            ReportDate = model.ReportDate,
            CreatedOn = DateTimeOffset.UtcNow.ToUniversalTime(),
            RebateAmount = model.RebateAmount.Truncate(2),
            PromotionAmount = model.PromotionAmount.Truncate(2),
            UpgradeAmount = model.UpgradeAmount.Truncate(2),
            SeasonAmount = model.SeasonAmount.Truncate(2),
            BirthdayAmount = model.BirthdayAmount.Truncate(2)
        }), cancellationToken).ConfigureAwait(false);

        return await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}