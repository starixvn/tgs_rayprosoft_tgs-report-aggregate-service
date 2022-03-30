using System;
using System.Threading.Tasks;
using Tgs.Report.Aggregate.Domain.Models;

namespace Tgs.Report.Aggregate.Domain.Interfaces
{
    public interface IPlayerBucketFactory
    {
        ValueTask FillInBucketAsync(DateTimeOffset start, DateTimeOffset end, RoundMinutelyModel[] request);
    }
}