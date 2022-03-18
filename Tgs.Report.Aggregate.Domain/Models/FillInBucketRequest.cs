using System;

namespace Tgs.Report.Aggregate.Domain.Models
{
    public record FillInPlayerBucketsRequest
    {
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
        public PlayerTurnoverModel[] PlayerTurnovers { get; init; }
    }

    public record PlayerTurnoverModel
    {
        public string PlayerCode { get; init; }
        public decimal Turnover { get; init; }
        public string Data { get; init; }
    }
}