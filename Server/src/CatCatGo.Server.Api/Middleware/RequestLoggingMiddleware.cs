using System.Diagnostics;

namespace CatCatGo.Server.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        var path = context.Request.Path;
        var method = context.Request.Method;
        var statusCode = context.Response.StatusCode;
        var elapsed = stopwatch.ElapsedMilliseconds;

        if (statusCode >= 500)
        {
            _logger.LogError("{Method} {Path} -> {StatusCode} ({Elapsed}ms)",
                method, path, statusCode, elapsed);
        }
        else if (statusCode >= 400)
        {
            _logger.LogWarning("{Method} {Path} -> {StatusCode} ({Elapsed}ms)",
                method, path, statusCode, elapsed);
        }
        else
        {
            _logger.LogInformation("{Method} {Path} -> {StatusCode} ({Elapsed}ms)",
                method, path, statusCode, elapsed);
        }
    }
}
