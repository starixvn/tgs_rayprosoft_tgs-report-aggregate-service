using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.Report.Aggregate.Core.Workers;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Service.MessageHandlers;
using Valhalla.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHostedService<PlayerAggregateBackground>()
    .AddHostedService<GameProviderAggregateBackground>()
    .AddHostedService<BrandGameProviderAggregateBackground>()
    .AddSingleton<IPlayerAggregateTaskQueue>(_ => new PlayerAggregateTaskQueue(100))
    .AddSingleton<IGameProviderAggregateTaskQueue>(_ => new GameProviderAggregateTaskQueue(100))
    .AddSingleton<IBrandGameProviderAggregateTaskQueue>(_ => new BrandGameProviderAggregateTaskQueue(100));

builder.Logging
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.ParseStateValues = true;
        _ = options.AddFluentdForwardExporter(fluentOptions =>
        {
            var nodeIp = builder.Configuration.GetValue<string>("NODE_IP");
            fluentOptions.Host = nodeIp!;
            fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
            fluentOptions.UseMessagePack();
        });
    });

builder.Services
    .AddOptions<OtlpExporterOptions>()
    .Configure<IConfiguration>((options, config) =>
    {
        var nodeIp = config.GetValue<string>("NODE_IP");

        if (!string.IsNullOrWhiteSpace(nodeIp))
            options.Endpoint = new Uri($"http://{nodeIp}:4317");

        config.GetSection("OpenTelemetry:Tracing").Bind(options);
    });

builder.Services
    .AddOpenTelemetry()
    .WithTracing(builder => builder
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddEnvironmentVariableDetector())
        .AddSource("Tgs.*")
        .AddSource("TGS.*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
        .AddProfileViewExporter()
        .AddOtlpExporter())
    .WithMetrics()
    .Services
    .Configure<AspNetCoreTraceInstrumentationOptions>(options => options.Filter =
        httpContext => !httpContext.Request.Path.HasValue ||
                       (!httpContext.Request.Path.StartsWithSegments("/swagger") &&
                        !httpContext.Request.Path.StartsWithSegments("/health") &&
                        !httpContext.Request.Path.StartsWithSegments("/profiler/") &&
                        !new[] { "html", "ico", "png", "jpg" }.Any(
                            ext => httpContext.Request.Path.Value.EndsWith($".{ext}"))));

builder.Services.AddProfileViewer();

builder.Services
    .AddTgsRepository(builder.Configuration.GetConnectionString("Tgs")!)
    .AddReportAggregateCore()
    .AddRedis((options, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        options.ConnectionString = config.GetConnectionString("RedisConfigurationString")!;
    })
    .AddPlayerBucketClient(httpClientBuilder => httpClientBuilder
        .ConfigureHttpClient(httpClient => httpClient.BaseAddress =
            builder.Configuration.GetValue<Uri>("ReportAggregate:PlayerBucketBaseUrl")))
    .AddBrandServiceClient((options, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        options.BrandCertificateBaseUrl = config.GetValue<Uri>("ReportAggregate:BrandCertificate:Url")!;
    });

builder.Services
    .AddPhoenixToolkitsMessageQueue()
    .AddNatsMessageQueue(natsConfig =>
    {
        _ = natsConfig
            .ConfigQueueOptions((options, sp) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();

                options.Url = configuration.GetConnectionString("Nats")!;
            });
        _ = natsConfig.AddHandler<BrandGameProviderAggregateMessageHandler>("tgs.report.aggregate.brand.game.provider",
            "tgs.report.aggregate");
        _ = natsConfig.AddHandler<GameProviderAggregateMessageHandler>("tgs.report.aggregate.game.provider",
            "tgs.report.aggregate");
    })
    .AddRabbitMessageQueue(config => config
        .ConfigQueueOptions((options, sp) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();

            configuration.GetSection("RabbitMQ").Bind(options);
        }))
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

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    ExceptionHandler = async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (exceptionFeature?.Error != null)
        {
            var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("ExceptionHandler");

            logger.LogError(
                exceptionFeature.Error,
                "{exceptionFeature.Error.Message}",
                exceptionFeature.Error.Message);

            var messageSender = app.Services.GetRequiredService<IMessageSender>();

            await messageSender.PublishAsync("report.error",
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new ReportErrorMessage(
                        $"{app.Environment.ApplicationName}.report.aggregate: {exceptionFeature.Error.StackTrace}"))))
                .ConfigureAwait(false);

            await context.Response.WriteAsJsonAsync(new MessageResponse
            {
                Message = exceptionFeature.Error.Message,
                Valid = false
            });
        }
    }
});
// Configure the HTTP request pipeline.
app.UseSwaggerUI();

app.MapControllers();
app.MapSwagger();
app.MapHealthChecks("/healthz");

app.Run();