using Serilog.Context;

namespace BookStore.Api.Middlewares;

public sealed class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var existingCorrelationId)
        ? existingCorrelationId.ToString()
        :Guid.NewGuid().ToString();



        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogInformation("Request started with correlation id {CorrelationId}", correlationId);

            await _next(context);
        }


    }





}