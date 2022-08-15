using System.Threading.Channels;

namespace Tgs.Report.Aggregate.Core.Workers
{
    public interface IBrandGameProviderAggregateTaskQueue
    {
        ValueTask QueueBrandGameProviderAggregateAsync(
            BrandGameProviderAggregateQueueModel model);

        ValueTask<BrandGameProviderAggregateQueueModel> DequeueAsync(
            CancellationToken cancellationToken);
    }

    public class BrandGameProviderAggregateTaskQueue : IBrandGameProviderAggregateTaskQueue
    {
        private readonly Channel<BrandGameProviderAggregateQueueModel> _queue;

        public BrandGameProviderAggregateTaskQueue(int capacity)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel
                .CreateBounded<BrandGameProviderAggregateQueueModel>(options);
        }

        public async ValueTask QueueBrandGameProviderAggregateAsync(
            BrandGameProviderAggregateQueueModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            await _queue.Writer.WriteAsync(model);
        }

        public async ValueTask<BrandGameProviderAggregateQueueModel>
            DequeueAsync(
                CancellationToken cancellationToken)
        {
            var workItem = await _queue.Reader.ReadAsync(cancellationToken);

            return workItem;
        }
    }
}