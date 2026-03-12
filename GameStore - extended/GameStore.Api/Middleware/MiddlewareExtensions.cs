public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();

    public static IApplicationBuilder UseMaintenance(this IApplicationBuilder app)
        => app.UseMiddleware<MaintenanceMiddleware>();

    // public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    //     => app.UseMiddleware<CorrelationIdMiddleware>();
}