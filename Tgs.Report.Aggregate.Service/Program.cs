using Tgs.Report.Aggregate.Service.MessageHandlers;
using Tgs.Report.Aggregate.Service.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((_, builder) =>
{
    builder.AddJsonFile("/config/appsettings.json", true);
});

builder.Services
    .AddTgsRepository(builder.Configuration.GetConnectionString("Tgs"))
    .AddReportAggregateCore()
    .AddRedis((options, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        options.ConnectionString = config.GetConnectionString("RedisConfigurationString");
    })
    .AddTgsLog();

builder.Services
    .AddMessageQueue()
    .AddNatsMessageQueue(natsConfig =>
    {
        natsConfig
            .ConfigQueueOptions((options, sp) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();

                options.Url = configuration.GetConnectionString("Nats");
            });
        natsConfig.AddHandler<BrandGameProviderAggregateMessageHandler>("tgs.report.aggregate.brand.game.provider",
            "tgs.report.aggregate");
        natsConfig.AddHandler<GameProviderAggregateMessageHandler>("tgs.report.aggregate.game.provider",
            "tgs.report.aggregate");
    })
    .AddRabbitMessageQueue(config =>
    {
        config
            .ConfigQueueOptions((options, sp) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();

                configuration.GetSection("RabbitMQ").Bind(options);
            });
    })
    .ClearAllExchanges()
    .AddRabbitGlobPatternExchange("report.error", "amq.direct")
    .AddNatsGlobPatternExchange("tgs.viplevel.aggregate.server")
    .AddNatsGlobPatternExchange("tgs.report.aggregate.brand.game.provider")
    .AddNatsGlobPatternExchange("tgs.report.aggregate.game.provider")
    .AddNatsGlobPatternExchange("*");

builder.Services.AddHealthChecks();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();