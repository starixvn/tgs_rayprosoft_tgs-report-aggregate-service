using System.Transactions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

namespace Tgs.Report.Aggregate.Core.Commands.PlayerAggregate
{
    public class
        ManualCreateUpdatePlayerAggregateHandler : IRequestHandler<ManualCreateUpdatePlayerAggregateModel,
            MessageResponse>
    {
        private readonly IPlayerAggregateFactory _playerAggregateFactory;
        private readonly IPlayerGameAggregateFactory _playerGameAggregateFactory;
        private readonly ITgsRepository _repository;
        private readonly IRoundAggregateFactory _roundAggregateFactory;

        public ManualCreateUpdatePlayerAggregateHandler(ITgsRepository repository,
            IPlayerAggregateFactory playerAggregateFactory, IPlayerGameAggregateFactory playerGameAggregateFactory,
            IRoundAggregateFactory roundAggregateFactory)
        {
            _repository = repository;
            _playerGameAggregateFactory = playerGameAggregateFactory;
            _playerAggregateFactory = playerAggregateFactory;
            _roundAggregateFactory = roundAggregateFactory;
        }

        public async Task<MessageResponse> Handle(ManualCreateUpdatePlayerAggregateModel request,
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

            var marketsOfDay = await _repository.PlayerAggregates
                .Where(x => x.ReportDate == reportDate.ToUniversalTime())
                .Where(x => x.Hour != request.Hour)
                .Select(x => x.Hour).Distinct().ToArrayAsync(cancellationToken);

            var hourArray = Enumerable.Range(0, result.Hour + 1);

            var doHours = hourArray.Except(marketsOfDay).ToArray();

            if (doHours.Length == 0)
            {
                return new MessageResponse { Valid = false, Message = "Not any hours to generate player aggregate." };
            }

            foreach (var doHour in doHours)
            {
                var start = new DateTimeOffset(result.Year, result.Month, result.Day, doHour,
                    0, 0, result.Offset);
                var end = new DateTimeOffset(result.Year, result.Month, result.Day, doHour,
                    0, 0, result.Offset).AddHours(1);

                if (await _repository.PlayerAggregates.AnyAsync(x =>
                        x.ReportDate == reportDate.ToUniversalTime() && x.Hour == start.Hour, cancellationToken))
                {
                    return new MessageResponse
                        { Valid = false, Message = $"{reportDate.Date} {start.Hour} is already existed." };
                }

                var (aggregates, aggregatesByGameType) =
                    await _roundAggregateFactory.GetPlayerAggregatesAsync(start, end);

                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                if (aggregates.Length > 0)
                {
                    await _playerAggregateFactory.CreatePlayerAggregatesAsync(aggregates);
                }

                if (aggregatesByGameType.Length > 0)
                {
                    await _playerGameAggregateFactory.CreatePlayerGameAggregatesAsync(
                        aggregatesByGameType);
                }

                scope.Complete();
            }

            return new() { Valid = true };
        }
    }
}