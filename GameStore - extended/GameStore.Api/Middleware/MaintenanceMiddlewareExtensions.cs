public static class MaintenanceMiddlewareExtensions
{
    public static IApplicationBuilder UseMaintenance(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MaintenanceMiddleware>();
    }
}