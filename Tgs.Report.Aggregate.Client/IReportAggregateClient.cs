using System.Threading.Tasks;

using Tgs.Report.Aggregate.Client.Models;

namespace Tgs.Report.Aggregate.Client
{
    public interface IReportAggregateClient
    {
        ValueTask<MessageResponse> CreatePlayerAggregateAsync(CreateUpdatePlayerAggregateModel request);

        ValueTask<MessageResponse> CreateRoundAggregateAsync(CreateRoundAggregateModel request);

        ValueTask<MessageResponse> FillPlayerBucketMinutelyAsync(FillPlayerBucketMinutelyModel request);

        ValueTask<MessageResponse> ManualCreatePlayerAggregateAsync(ManualCreateUpdatePlayerAggregateModel request);

        ValueTask<MessageResponse> ManualCreateRoundAggregateAsync(ManualCreateUpdateRoundAggregateModel request);

        ValueTask<MessageResponse> ManualFillPlayerBucketMinutelyAsync(ManualFillPlayerBucketMinutelyModel request);
    }
}
