using System;
using MediatR;

namespace Tgs.Report.Aggregate.Domain.Models.GameProviderAggregate;

public record RemoveGameProviderAggregateRequest
	(Guid? GameProviderId, DateTimeOffset StartDate) : IRequest<MessageResponse>;
