using System;

namespace Tgs.Report.Aggregate.Client.Models;

public record RemoveBrandGameProviderAggregateRequest(Guid? BrandId, DateTimeOffset StartDate);