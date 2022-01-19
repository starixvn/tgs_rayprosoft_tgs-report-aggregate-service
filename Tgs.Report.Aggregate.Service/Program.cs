using Tgs.Report.Aggregate.Service.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((_, builder) =>
{
    builder.AddJsonFile("/config/appsettings.json", true);
});

builder.Services
    .AddTgsRepository(builder.Configuration.GetConnectionString("Tgs"))
    .AddReportAggregateCore()
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
    })
    .AddRabbitMessageQueue(config =>
    {
        config
            .ConfigQueueOptions((options, sp) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();

                options.RabbitMQUrl = configuration.GetValue<Uri>("ConnectionStrings:RabbitMQ");
            });
    })
    .ClearAllExchanges()
    .AddRabbitGlobPatternExchange("report.error", "amq.direct")
    .AddNatsGlobPatternExchange("tgs.viplevel.aggregate.server")
    .AddNatsGlobPatternExchange("*");

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();