using System.Threading.Channels;

namespace Tgs.Report.Aggregate.Core.Workers;

public interface IGeneratePlayerAggregateTaskQueue
{
    ValueTask QueueGeneratePlayerAggregateAsync(GeneratePlayerAggregateQueueModel model);

    ValueTask<GeneratePlayerAggregateQueueModel> DequeueAsync(CancellationToken cancellationToken);
}

public class GeneratePlayerAggregateTaskQueue : IGeneratePlayerAggregateTaskQueue
{
    private readonly Channel<GeneratePlayerAggregateQueueModel> _queue;

    public GeneratePlayerAggregateTaskQueue(int capacity)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<GeneratePlayerAggregateQueueModel>(options);
    }

    public async ValueTask QueueGeneratePlayerAggregateAsync(GeneratePlayerAggregateQueueModel model)
    {
        if(model == null) throw new ArgumentNullException(nameof(model));

        await _queue.Writer.WriteAsync(model);
    }

    public async ValueTask<GeneratePlayerAggregateQueueModel> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);

        return workItem;
    }
}
