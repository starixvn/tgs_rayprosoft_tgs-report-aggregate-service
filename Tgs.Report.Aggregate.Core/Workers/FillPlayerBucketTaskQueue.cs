using System.Threading.Channels;

namespace Tgs.Report.Aggregate.Core.Workers;

    public interface IFillPlayerBucketTaskQueue
{
        ValueTask QueueFillPlayerBucketAsync(
            FillPlayerBucketQueueModel model);

        ValueTask<FillPlayerBucketQueueModel> DequeueAsync(
            CancellationToken cancellationToken);
    }

    public class FillPlayerBucketTaskQueue : IFillPlayerBucketTaskQueue
{
        private readonly Channel<FillPlayerBucketQueueModel> _queue;

        public FillPlayerBucketTaskQueue()
        {
            _queue = Channel.CreateUnbounded<FillPlayerBucketQueueModel>(new UnboundedChannelOptions());
        }

        public async ValueTask QueueFillPlayerBucketAsync(
            FillPlayerBucketQueueModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            await _queue.Writer.WriteAsync(model);
        }

        public async ValueTask<FillPlayerBucketQueueModel>
            DequeueAsync(
                CancellationToken cancellationToken)
        {
            var workItem = await _queue.Reader.ReadAsync(cancellationToken);

            return workItem;
        }
    }