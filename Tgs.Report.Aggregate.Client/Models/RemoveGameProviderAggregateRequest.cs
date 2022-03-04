using System;

namespace Tgs.Report.Aggregate.Client.Models
{
    public record RemoveGameProviderAggregateRequest(Guid? GameProviderId, DateTimeOffset StartDate);
}