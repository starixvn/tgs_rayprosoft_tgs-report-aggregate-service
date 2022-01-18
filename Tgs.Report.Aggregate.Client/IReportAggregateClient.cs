using System.Net.Http;
using System.Threading.Tasks;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Client.Models;

namespace TGS.Clients.Report.Client
{
    public interface IReportAggregateClient
    {
        ValueTask<MessageResponse> CreatePlayerAggregateAsync(CreateUpdatePlayerAggregateModel request);
        ValueTask<MessageResponse> ManualCreatePlayerAggregateAsync(ManualCreateUpdatePlayerAggregateModel request);
        ValueTask<MessageResponse> CreateRoundAggregateAsync(CreateRoundAggregateModel request);
        ValueTask<MessageResponse> FillPlayerBucketMinutelyAsync(FillPlayerBucketMinutelyModel request);
        ValueTask<MessageResponse> ManualFillPlayerBucketMinutelyAsync(ManualFillPlayerBucketMinutelyModel request);
        ValueTask<MessageResponse> ManualCreateRoundAggregateAsync(ManualCreateUpdateRoundAggregateModel request);
    }
}