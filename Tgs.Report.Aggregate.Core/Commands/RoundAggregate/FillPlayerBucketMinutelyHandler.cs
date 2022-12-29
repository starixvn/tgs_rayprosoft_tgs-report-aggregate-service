using MediatR;
using Microsoft.Extensions.Logging;
using Tgs.Report.Aggregate.Core.Workers;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

namespace Tgs.Report.Aggregate.Core.Commands.RoundAggregate;

public class FillPlayerBucketMinutelyHandler : IRequestHandler<FillPlayerBucketMinutelyRequest, MessageResponse>
{
    private readonly IFillPlayerBucketTaskQueue _taskQueue;
	private readonly IRoundFactory _roundFactory;
    private readonly ILogger<FillPlayerBucketMinutelyHandler> _logger;

    public FillPlayerBucketMinutelyHandler(
        IFillPlayerBucketTaskQueue taskQueue,
		IRoundFactory roundFactory,
        ILogger<FillPlayerBucketMinutelyHandler> logger)
	{
        _taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
		_roundFactory = roundFactory ?? throw new ArgumentNullException(nameof(roundFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

    public async Task<MessageResponse> Handle(FillPlayerBucketMinutelyRequest request,
		CancellationToken cancellationToken)
	{
		var previousMinute = request.StartDate.ToOffset(new(8, 0, 0));

		var start = new DateTimeOffset(previousMinute.Year, previousMinute.Month, previousMinute.Day,
			previousMinute.Hour,
			previousMinute.Minute, 0, previousMinute.Offset);

		var end = new DateTimeOffset(previousMinute.Year, previousMinute.Month, previousMinute.Day,
			previousMinute.Hour,
			previousMinute.Minute, 0, previousMinute.Offset).AddMinutes(1);

		var models = await _roundFactory.GetMinuteRoundAggregateAsync(start, end);

        foreach (var model in models)
        {
            await _taskQueue.QueueFillPlayerBucketAsync(new FillPlayerBucketQueueModel(
                model.RoundId, 
                model.PlayerCode,
                model.Turnover,
                start));
        }

        return new() { Valid = true };
	}
}
