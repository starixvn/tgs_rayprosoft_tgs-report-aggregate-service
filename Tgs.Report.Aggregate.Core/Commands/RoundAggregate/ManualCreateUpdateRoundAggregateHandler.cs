using MediatR;

using Microsoft.EntityFrameworkCore;

using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;
using Tgs.Report.Aggregate.Repository.Repositories;

namespace Tgs.Report.Aggregate.Core.Commands.RoundAggregate
{
    public class
        ManualCreateUpdateRoundAggregateHandler : IRequestHandler<ManualCreateRoundAggregateModel,
            MessageResponse>
    {
        private readonly ITgsRepository _repository;
        private readonly IRoundFactory _roundFactory;

        public ManualCreateUpdateRoundAggregateHandler(
            ITgsRepository repository,
            IRoundFactory roundFactory)
        {
            _roundFactory = roundFactory;
            _repository = repository;
        }

        public async Task<MessageResponse> Handle(ManualCreateRoundAggregateModel request,
            CancellationToken cancellationToken)
        {
            DateTimeOffset reportDate;

            if (!string.IsNullOrEmpty(request.Date) &&
                DateTimeOffset.TryParse($"{request.Date} {request.Hour}:00 +08:00", out var result))
            {
                reportDate = new(result.Year, result.Month, result.Day, 0, 0, 0,
                    result.Offset);
            }
            else
            {
                return new MessageResponse { Valid = false, Message = "Date is invalid." };
            }

            var marketsOfDay = await _repository.RoundAggregates
                .Where(x => x.ReportDate == reportDate.ToUniversalTime())
                .Select(x => x.Hour).Distinct().ToArrayAsync(cancellationToken);

            var hourArray = Enumerable.Range(0, result.Hour + 1);

            var doHours = hourArray.Except(marketsOfDay).ToArray();

            if (doHours.Length == 0)
            {
                return new MessageResponse { Valid = false, Message = "Not any hours to generate round aggregate." };
            }

            foreach (var doHour in doHours)
            {
                var start = new DateTimeOffset(result.Year, result.Month, result.Day, doHour,
                    0, 0, result.Offset);
                var end = new DateTimeOffset(result.Year, result.Month, result.Day, doHour,
                    0, 0, result.Offset).AddHours(1);

                if (await _repository.RoundAggregates.AnyAsync(x =>
                        x.ReportDate == reportDate.ToUniversalTime() && x.Hour == start.Hour, cancellationToken) ||
                    end > DateTimeOffset.Now)
                {
                    continue;
                }

                await _roundFactory.GenerateRoundAggregateAsync(start, end);
            }

            return new() { Valid = true };
        }
    }
}
