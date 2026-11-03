namespace Gateway.Api;

public sealed class CorrelationMiddleware
{
    private const string HeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
            context.Request.Headers[HeaderName] = correlationId;
        }

        context.Response.Headers[HeaderName] = correlationId;
        context.TraceIdentifier = correlationId;
        await _next(context);
    }
}
