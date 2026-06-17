using System.Diagnostics;

namespace ECommerce.Gateway.Middleware;

/// <summary>
/// Logs every request that comes through the gateway.
/// Adds a unique X-Request-Id header so you can trace a request
/// across all service logs — invaluable for debugging in production.
///
/// Log format: [METHOD] /path → downstream service → {status} in {ms}ms
/// </summary>
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
        // Assign unique ID to every request — attach to all downstream calls
        var requestId = context.Request.Headers["X-Request-Id"].FirstOrDefault()
                        ?? Guid.NewGuid().ToString("N")[..8]; // Short 8-char ID

        context.Request.Headers["X-Request-Id"] = requestId;
        context.Response.Headers["X-Request-Id"] = requestId;

        var sw = Stopwatch.StartNew();
        var path = context.Request.Path;
        var method = context.Request.Method;

        _logger.LogInformation(
            "[GATEWAY] → [{Method}] {Path} | RequestId: {RequestId}",
            method, path, requestId);

        await _next(context);

        sw.Stop();
        var statusCode = context.Response.StatusCode;

        if (statusCode >= 400)
        {
            _logger.LogWarning(
                "[GATEWAY] ← [{Method}] {Path} | {StatusCode} | {Elapsed}ms | RequestId: {RequestId}",
                method, path, statusCode, sw.ElapsedMilliseconds, requestId);
        }
        else
        {
            _logger.LogInformation(
                "[GATEWAY] ← [{Method}] {Path} | {StatusCode} | {Elapsed}ms | RequestId: {RequestId}",
                method, path, statusCode, sw.ElapsedMilliseconds, requestId);
        }
    }
}
