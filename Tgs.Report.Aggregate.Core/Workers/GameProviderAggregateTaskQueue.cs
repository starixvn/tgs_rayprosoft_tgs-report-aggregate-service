using System.Threading.Channels;

namespace Tgs.Report.Aggregate.Core.Workers;

    public interface IGameProviderAggregateTaskQueue
    {
        ValueTask QueueGameProviderAggregateAsync(
            GameProviderAggregateQueueModel model);

        ValueTask<GameProviderAggregateQueueModel> DequeueAsync(
            CancellationToken cancellationToken);
    }

    public class GameProviderAggregateTaskQueue : IGameProviderAggregateTaskQueue
    {
        private readonly Channel<GameProviderAggregateQueueModel> _queue;

        public GameProviderAggregateTaskQueue(int capacity)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel
                .CreateBounded<GameProviderAggregateQueueModel>(options);
        }

        public async ValueTask QueueGameProviderAggregateAsync(
            GameProviderAggregateQueueModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            await _queue.Writer.WriteAsync(model);
        }

        public async ValueTask<GameProviderAggregateQueueModel>
            DequeueAsync(
                CancellationToken cancellationToken)
        {
            var workItem = await _queue.Reader.ReadAsync(cancellationToken);

            return workItem;
        }
    }