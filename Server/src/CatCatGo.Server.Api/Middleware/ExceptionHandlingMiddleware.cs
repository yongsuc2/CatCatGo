using System.Text.Json;
using CatCatGo.Shared.Models;

namespace CatCatGo.Server.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.Items["CorrelationId"] as string ?? "unknown";
            _logger.LogError(ex, "Unhandled exception for {Method} {Path} [CorrelationId={CorrelationId}]",
                context.Request.Method, context.Request.Path, correlationId);

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var response = ApiResponse.Fail("INTERNAL_ERROR", "An unexpected error occurred.");
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
        }
    }
}
