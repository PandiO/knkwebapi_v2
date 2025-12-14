using System.Diagnostics;
using knkwebapi_v2.Models.ClientActivity;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Middleware;

/// <summary>
/// Middleware that tracks client application activity.
/// Parses client headers (X-Knk-Client-*) and records request metrics.
/// Minimal overhead; should be placed early in the pipeline.
/// </summary>
public class ClientActivityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ClientActivityMiddleware> _logger;

    public ClientActivityMiddleware(RequestDelegate next, ILogger<ClientActivityMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IClientActivityStore clientActivityStore)
    {
        // Extract client information from headers.
        var client = ExtractClientInfo(context.Request.Headers);

        // Skip recording for health check endpoints to avoid noise.
        if (context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Path.StartsWithSegments("/_internal", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Start stopwatch to measure request duration without touching the response body.
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _next(context);
            stopwatch.Stop();

            var request = new RequestInfo
            {
                Method = context.Request.Method,
                RouteTemplate = context.GetRouteTemplate(),
                StatusCode = context.Response.StatusCode,
                DurationMs = stopwatch.ElapsedMilliseconds,
                TimestampUtc = DateTime.UtcNow,
            };

            clientActivityStore.RecordRequest(client, request);

            _logger.LogTrace(
                "Client {ClientType}/{ClientId} {Method} {Route} -> {StatusCode} ({DurationMs}ms)",
                client.ClientType, client.ClientId, request.Method, request.RouteTemplate,
                request.StatusCode, request.DurationMs);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var request = new RequestInfo
            {
                Method = context.Request.Method,
                RouteTemplate = context.GetRouteTemplate(),
                StatusCode = context.Response.StatusCode > 0 ? context.Response.StatusCode : 500,
                DurationMs = stopwatch.ElapsedMilliseconds,
                TimestampUtc = DateTime.UtcNow,
            };

            try
            {
                clientActivityStore.RecordRequest(client, request);
            }
            catch (Exception storeEx)
            {
                _logger.LogError(storeEx, "Error recording client activity");
            }

            _logger.LogError(ex, "Unhandled exception during request processing");
            throw;
        }
    }

    /// <summary>
    /// Extracts client information from request headers.
    /// Missing headers result in "unknown" client type.
    /// </summary>
    private static ClientInfo ExtractClientInfo(IHeaderDictionary headers)
    {
        var clientId = headers["X-Knk-Client-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        var clientType = headers["X-Knk-Client-Type"].FirstOrDefault() ?? "unknown";
        var clientVersion = headers["X-Knk-Client-Version"].FirstOrDefault();
        var clientName = headers["X-Knk-Client-Name"].FirstOrDefault();

        return new ClientInfo
        {
            ClientId = clientId,
            ClientType = clientType,
            ClientVersion = clientVersion,
            ClientName = clientName,
        };
    }
}

/// <summary>
/// Extension methods for client activity tracking.
/// </summary>
public static class ClientActivityMiddlewareExtensions
{
    /// <summary>
    /// Adds client activity tracking middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseClientActivityTracking(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ClientActivityMiddleware>();
    }

    /// <summary>
    /// Gets the route template for the current request, or the full path if no route is matched.
    /// </summary>
    public static string GetRouteTemplate(this HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            // Try to get the route pattern.
            var routePattern = endpoint.Metadata.OfType<Microsoft.AspNetCore.Routing.RouteNameMetadata>().FirstOrDefault();
            if (routePattern != null)
                return routePattern.RouteName ?? context.Request.Path.Value ?? "/";

            // Try to get the display name (e.g., controller action).
            var displayName = endpoint.DisplayName;
            if (!string.IsNullOrEmpty(displayName))
                return displayName;
        }

        // Fall back to request path (without query string).
        return context.Request.Path.Value ?? "/";
    }
}
