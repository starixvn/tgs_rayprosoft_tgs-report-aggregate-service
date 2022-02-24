using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Controllers;

using Tgs.Logging;
using Tgs.MessageQueue;
using Tgs.Report.Aggregate.Domain;

namespace Tgs.Report.Aggregate.Service.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly IWebHostEnvironment _env;
        private readonly IMessageSender _messageSender;

        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next, IMessageSender messageSender, IWebHostEnvironment env)
        {
            _next = next;
            _env = env;
            _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        }

        public async Task InvokeAsync(HttpContext context, ITgsLog log)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                var endpoint = context.GetEndpoint();
                var name = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>()?.MethodInfo.Name;

                log.SetLogger(name);
                log.LogError(
                    $"Report api error on :{context.Request.Host} with message: {exception.Message} : {exception.InnerException?.Message}",
                    exception);

                await _messageSender.PublishAsync("report.error",
                        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new ReportErrorMessage(
                            $"{_env.EnvironmentName}: {exception.Message}"))))
                    .ConfigureAwait(false);

                await context.Response.WriteAsJsonAsync(new MessageResponse
                {
                    Message = exception.Message,
                    Valid = false
                });
            }
        }
    }
}
