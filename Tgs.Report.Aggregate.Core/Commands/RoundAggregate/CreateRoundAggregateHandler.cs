using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;
using Tgs.Report.Aggregate.Repository.Repositories;

namespace Tgs.Report.Aggregate.Core.Commands.RoundAggregate
{
    public class
        CreateRoundAggregateHandler : IRequestHandler<CreateRoundAggregateRequest,
            MessageResponse>
    {
        private readonly ITgsRepository _repository;
        private readonly IRoundFactory _roundFactory;

        public CreateRoundAggregateHandler(ITgsRepository repository,
            IRoundFactory roundFactory)
        {
            _roundFactory = roundFactory;
            _repository = repository;
        }

        public async Task<MessageResponse> Handle(CreateRoundAggregateRequest request,
            CancellationToken cancellationToken)
        {
            var previousHour = DateTimeOffset.Now.ToOffset(new(8, 0, 0)).AddHours(-1);
            var theDay = new DateTimeOffset(previousHour.Year, previousHour.Month, previousHour.Day, 0, 0, 0,
                previousHour.Offset).ToUniversalTime();

            var marketsOfDay = await _repository.RoundAggregates
                .Where(x => x.ReportDate == theDay)
                .Select(x => x.Hour).Distinct().ToArrayAsync(cancellationToken);

            var hourArray = Enumerable.Range(0, previousHour.Hour + 1);

            var doHours = hourArray.Except(marketsOfDay).ToArray();

            foreach (var doHour in doHours)
            {
                var start = new DateTimeOffset(previousHour.Year, previousHour.Month, previousHour.Day, doHour,
                    0, 0, previousHour.Offset).ToUniversalTime();
                var end = new DateTimeOffset(previousHour.Year, previousHour.Month, previousHour.Day, doHour,
                    0, 0, previousHour.Offset).AddHours(1).ToUniversalTime();

                if (await _repository.RoundAggregates.AnyAsync(x =>
                        x.ReportDate == theDay && x.Hour == start.Hour, cancellationToken))
                {
                    return new MessageResponse
                    {
                        Valid = false,
                        Message = $"{theDay.Date} {start.Hour} is already existed."
                    };
                }

                await _roundFactory.GenerateRoundAggregateAsync(start, end);
            }

            return new() { Valid = true };
        }
    }
}