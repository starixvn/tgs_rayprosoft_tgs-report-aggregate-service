using System;
using MediatR;

namespace Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

public record FillPlayerBucketMinutelyRequest(DateTimeOffset StartDate) : IRequest<MessageResponse>;
