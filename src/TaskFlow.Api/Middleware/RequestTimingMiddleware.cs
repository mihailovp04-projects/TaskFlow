using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Options;
using TaskFlow.Api.Options;

namespace TaskFlow.Api.Middleware;

public sealed class RequestTimingMiddleware(
    RequestDelegate next,
    ILogger<RequestTimingMiddleware> logger,
    IOptions<RequestTimingOptions> options)
{
    public const string HeaderName = "X-Elapsed-Ms";

    public async Task InvokeAsync(HttpContext context)
    {
        var settings = options.Value;
        var stopwatch = Stopwatch.StartNew();

        if (settings.AddResponseHeader)
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] =
                    stopwatch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture);
                return Task.CompletedTask;
            });
        }

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            var method = context.Request.Method;
            var path = context.Request.Path.Value;
            var statusCode = context.Response.StatusCode;

            if (elapsedMs >= settings.SlowRequestThresholdMs)
            {
                logger.LogWarning("Slow request {Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
                    method, path, statusCode, elapsedMs);
            }
            else
            {
                logger.LogInformation("{Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
                    method, path, statusCode, elapsedMs);
            }
        }
    }
}
