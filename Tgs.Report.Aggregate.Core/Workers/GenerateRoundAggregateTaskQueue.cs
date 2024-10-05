using System.Threading.Channels;

namespace Tgs.Report.Aggregate.Core.Workers;

public interface IGenerateRoundAggregateTaskQueue
{
    ValueTask QueueRoundUpdateAggregateAsync(GenerateRoundAggregateQueueModel model);

    ValueTask<GenerateRoundAggregateQueueModel> DequeueAsync(CancellationToken cancellationToken);
}

public class GenerateRoundAggregateTaskQueue : IGenerateRoundAggregateTaskQueue
{
    private readonly Channel<GenerateRoundAggregateQueueModel> _queue;

    public GenerateRoundAggregateTaskQueue(int capacity)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<GenerateRoundAggregateQueueModel>(options);
    }

    public async ValueTask QueueRoundUpdateAggregateAsync(GenerateRoundAggregateQueueModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        await _queue.Writer.WriteAsync(model);
    }

    public async ValueTask<GenerateRoundAggregateQueueModel> DequeueAsync(CancellationToken cancellationToken)
    {
       var workItem = await _queue.Reader.ReadAsync(cancellationToken);

        return workItem;
    }
}
