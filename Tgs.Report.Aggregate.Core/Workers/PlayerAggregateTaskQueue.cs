using System.Threading.Channels;

namespace Tgs.Report.Aggregate.Core.Workers;

public interface IPlayerAggregateTaskQueue
{
    ValueTask QueuePlayerAggregateAsync(
        PlayerAggregateQueueModel model);

    ValueTask<PlayerAggregateQueueModel> DequeueAsync(
        CancellationToken cancellationToken);
}

public class PlayerAggregateTaskQueue : IPlayerAggregateTaskQueue
{
    private readonly Channel<PlayerAggregateQueueModel> _queue;

    public PlayerAggregateTaskQueue(int capacity)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel
            .CreateBounded<PlayerAggregateQueueModel>(options);
    }

    public async ValueTask QueuePlayerAggregateAsync(
        PlayerAggregateQueueModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        await _queue.Writer.WriteAsync(model);
    }

    public async ValueTask<PlayerAggregateQueueModel>
        DequeueAsync(
            CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);

        return workItem;
    }
}