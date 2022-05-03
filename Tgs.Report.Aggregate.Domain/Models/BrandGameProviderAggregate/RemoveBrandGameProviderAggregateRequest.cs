using System;
using MediatR;

namespace Tgs.Report.Aggregate.Domain.Models.BrandGameProviderAggregate;

public record RemoveBrandGameProviderAggregateRequest
	(Guid? BrandId, DateTimeOffset StartDate) : IRequest<MessageResponse>;
