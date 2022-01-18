using System.Text;
using System.Text.Json;
using System.Transactions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.MessageQueue;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;
using Tgs.Report.Aggregate.Repository.Repositories;

namespace Tgs.Report.Aggregate.Core.Commands.PlayerAggregate
{
    public class
        CreateUpdatePlayerAggregateHandler : IRequestHandler<CreateUpdatePlayerAggregateModel,
            MessageResponse>
    {
        private readonly IMessageSender _messageSender;
        private readonly IPlayerAggregateFactory _playerAggregateFactory;
        private readonly IPlayerGameAggregateFactory _playerGameAggregateFactory;
        private readonly ITgsRepository _repository;
        private readonly IRoundAggregateFactory _roundAggregateFactory;

        public CreateUpdatePlayerAggregateHandler(
            ITgsRepository repository,
            IMessageSender messageSender,
            IRoundAggregateFactory roundAggregateFactory,
            IPlayerAggregateFactory playerAggregateFactory,
            IPlayerGameAggregateFactory playerGameAggregateFactory)
        {
            _repository = repository;
            _messageSender = messageSender;
            _roundAggregateFactory = roundAggregateFactory;
            _playerAggregateFactory = playerAggregateFactory;
            _playerGameAggregateFactory = playerGameAggregateFactory;
        }

        public async Task<MessageResponse> Handle(CreateUpdatePlayerAggregateModel request,
            CancellationToken cancellationToken)
        {
            var previousHour = DateTimeOffset.Now.ToOffset(new(8, 0, 0)).AddHours(-1);
            var reportDate = new DateTimeOffset(previousHour.Year,
                previousHour.Month,
                previousHour.Day,
                0,
                0,
                0,
                previousHour.Offset).ToUniversalTime();

            var marketsOfDay = await _repository.PlayerAggregates
                .Where(x => x.ReportDate == reportDate)
                .Select(x => x.Hour).Distinct().ToArrayAsync(cancellationToken);

            var hourArray = Enumerable.Range(0, previousHour.Hour + 1);

            var doHours = hourArray.Except(marketsOfDay).ToArray();

            if (doHours.Length == 0)
            {
                return new MessageResponse
                {
                    Valid = false,
                    Message = "Not any hours to generate player aggregate."
                };
            }

            var playerCodes = Array.Empty<string>();
            foreach (var doHour in doHours)
            {
                var start = new DateTimeOffset(previousHour.Year,
                    previousHour.Month,
                    previousHour.Day,
                    doHour,
                    0,
                    0,
                    previousHour.Offset);
                var end = new DateTimeOffset(previousHour.Year,
                    previousHour.Month,
                    previousHour.Day,
                    doHour,
                    0,
                    0,
                    previousHour.Offset).AddHours(1);

                if (await _repository.PlayerAggregates.AnyAsync(x =>
                        x.ReportDate == reportDate && x.Hour == start.Hour, cancellationToken))
                {
                    return new MessageResponse
                    {
                        Valid = false,
                        Message = $"{reportDate.Date} {start.Hour} is already existed."
                    };
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
                    await _playerGameAggregateFactory.CreatePlayerGameAggregatesAsync(aggregatesByGameType);
                }

                playerCodes = playerCodes.Union(aggregates.Select(x => x.PlayerCode)).ToArray();

                scope.Complete();
            }

            var sendForVipLevelUpCalculation = new
            {
                ReportOn = reportDate,
                PlayerCodes = playerCodes
            };

            await _messageSender.PublishAsync("tgs.viplevel.aggregate.server",
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sendForVipLevelUpCalculation)))
                .ConfigureAwait(false);

            return new() { Valid = true };
        }
    }
}