using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace testing.Middleware
{
    public class SignalRLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SignalRLoggingMiddleware> _logger;

        public SignalRLoggingMiddleware(RequestDelegate next, ILogger<SignalRLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/hubs"))
            {
                var stopwatch = Stopwatch.StartNew();
                var connectionId = context.Connection.Id;

                _logger.LogInformation($"SignalR Connection: {connectionId} - Path: {context.Request.Path}");

                try
                {
                    await _next(context);
                }
                finally
                {
                    stopwatch.Stop();
                    _logger.LogInformation($"SignalR Connection completed: {connectionId} - Duration: {stopwatch.ElapsedMilliseconds}ms");
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}