using MediatR;

namespace Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

public record ManualFillPlayerBucketMinutelyRequest
	(string Date, int Hour, int FromMinute, int ToMinute, bool FillPlayerBucket) : IRequest<MessageResponse>;
