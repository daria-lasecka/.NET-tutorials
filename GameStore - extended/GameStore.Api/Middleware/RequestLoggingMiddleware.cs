// TODO: in which folder?
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
        _logger.LogInformation($"Incoming Request: {context.Request.Method} {context.Request.Path}");

        await _next(context); // Continue the pipeline

        _logger.LogInformation($"Response Status Code: {context.Response.StatusCode}");
    }
}